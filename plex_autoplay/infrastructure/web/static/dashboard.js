(() => {
  const csrfToken = document.querySelector('meta[name="csrf-token"]')?.content || "";
  const scanButton = document.querySelector("#scan-button");
  const statusBanner = document.querySelector("#scan-status");
  const statusMessage = document.querySelector("#status-message");
  let lastCompletedAt = statusBanner?.dataset.completedAt || "";
  let polling = false;

  async function requestJson(url, options = {}) {
    const response = await fetch(url, {
      credentials: "same-origin",
      ...options,
    });
    if (response.status === 204) {
      if (!response.ok) throw new Error(`A solicitação falhou (HTTP ${response.status}).`);
      return null;
    }
    const payload = await response.json();
    if (!response.ok) {
      const error = new Error(payload.error || `A solicitação falhou (HTTP ${response.status}).`);
      error.payload = payload;
      error.status = response.status;
      throw error;
    }
    return payload;
  }

  function setStatus(snapshot) {
    if (!statusBanner || !statusMessage) return;
    const labels = {
      idle: "Nenhuma varredura registrada.",
      queued: "Varredura na fila.",
      running: "Varredura em andamento.",
      completed: `Última varredura concluída${snapshot.completed_at ? ` em ${snapshot.completed_at}` : ""}.`,
      failed: `A última varredura falhou: ${snapshot.error || "erro não informado."}`,
    };
    if (!Object.hasOwn(labels, snapshot.status)) {
      throw new Error(`Estado de varredura desconhecido: ${snapshot.status}`);
    }
    statusBanner.className = `status-banner status-${snapshot.status}`;
    statusMessage.textContent = labels[snapshot.status];
    if (scanButton) {
      const active = snapshot.status === "queued" || snapshot.status === "running";
      scanButton.disabled = active;
      scanButton.setAttribute("aria-busy", String(active));
      scanButton.querySelector("span").textContent = active ? "Varredura em andamento" : "Executar varredura";
    }
    if (
      snapshot.status === "completed"
      && snapshot.completed_at
      && snapshot.completed_at !== lastCompletedAt
    ) {
      lastCompletedAt = snapshot.completed_at;
      window.location.reload();
    } else if (snapshot.completed_at) {
      lastCompletedAt = snapshot.completed_at;
    }
  }

  async function refreshStatus() {
    if (!statusBanner || polling) return;
    polling = true;
    try {
      setStatus(await requestJson("/api/status"));
    } catch (error) {
      statusBanner.className = "status-banner status-failed";
      statusMessage.textContent = `Não foi possível atualizar o estado: ${error.message}`;
    } finally {
      polling = false;
    }
  }

  if (scanButton) {
    scanButton.addEventListener("click", async () => {
      scanButton.disabled = true;
      try {
        const snapshot = await requestJson("/api/scan", {
          method: "POST",
          headers: { "X-CSRF-Token": csrfToken },
        });
        setStatus(snapshot);
      } catch (error) {
        statusMessage.textContent = error.message;
        await refreshStatus();
      }
    });
    window.setInterval(refreshStatus, 5000);
  }

  for (const form of document.querySelectorAll(".marker-form")) {
    const feedback = form.querySelector(".marker-feedback");
    const identity = {
      series: form.dataset.series,
      season: Number(form.dataset.season),
      episode: Number(form.dataset.episode),
    };

    async function saveMarkers(confirmReplace) {
      const values = {};
      for (const input of form.querySelectorAll('input[name="start"], input[name="end"], input[name="credits"]')) {
        if (input.value !== "") values[input.name] = Number(input.value);
      }
      if (Object.keys(values).length === 0) {
        feedback.textContent = "Informe pelo menos um marcador. Para limpar, selecione campos em Remover.";
        feedback.classList.add("is-error");
        return;
      }
      const result = await requestJson("/api/markers", {
        method: "PUT",
        headers: { "Content-Type": "application/json", "X-CSRF-Token": csrfToken },
        body: JSON.stringify({ ...identity, values, confirm_replace: confirmReplace }),
      });
      feedback.classList.remove("is-error");
      feedback.textContent = result.panel_updated
        ? "Marcadores salvos; atualizando o painel."
        : "Marcadores salvos; a cobertura aparecerá após a próxima varredura.";
      if (result.panel_updated) window.location.reload();
    }

    form.addEventListener("submit", async (event) => {
      event.preventDefault();
      try {
        await saveMarkers(false);
      } catch (error) {
        if (
          error.payload?.confirmation_required
          && window.confirm(`${error.message} Deseja substituir os valores existentes?`)
        ) {
          try {
            await saveMarkers(true);
          } catch (retryError) {
            feedback.textContent = retryError.message;
            feedback.classList.add("is-error");
          }
        } else {
          feedback.textContent = error.message;
          feedback.classList.add("is-error");
        }
      }
    });

    form.querySelector(".remove-markers").addEventListener("click", async () => {
      const fields = [...form.querySelectorAll(".marker-removal-fields input:checked")].map((input) => input.value);
      if (fields.length === 0) {
        feedback.textContent = "Selecione os campos que deseja remover.";
        feedback.classList.add("is-error");
        return;
      }
      if (!window.confirm(`Remover apenas: ${fields.join(", ")}?`)) return;
      try {
        const result = await requestJson("/api/markers", {
          method: "DELETE",
          headers: { "Content-Type": "application/json", "X-CSRF-Token": csrfToken },
          body: JSON.stringify({ ...identity, fields }),
        });
        feedback.classList.remove("is-error");
        feedback.textContent = result.panel_updated
          ? "Campos selecionados removidos; atualizando o painel."
          : "Campos removidos; a cobertura aparecerá após a próxima varredura.";
        if (result.panel_updated) window.location.reload();
      } catch (error) {
        feedback.textContent = error.message;
        feedback.classList.add("is-error");
      }
    });
  }

  const libraryForm = document.querySelector("#library-form");
  if (!libraryForm) return;

  const directorySelect = document.querySelector("#directory-select");
  const directoryLocation = document.querySelector("#directory-location");
  const addButton = document.querySelector("#add-library");
  const feedback = document.querySelector("#manager-feedback");
  const libraryList = document.querySelector("#configured-libraries");
  const listFeedback = document.querySelector("#library-list-feedback");
  let currentDirectory = "";

  function showFeedback(element, message, isError = false) {
    element.textContent = message;
    element.classList.toggle("is-error", isError);
  }

  async function loadDirectories(path = "") {
    const query = path ? `?path=${encodeURIComponent(path)}` : "";
    const result = await requestJson(`/api/directories${query}`);
    currentDirectory = path;
    directoryLocation.textContent = path || "Raiz de mídia";
    directorySelect.replaceChildren(new Option("Escolha uma subpasta", ""));
    for (const directory of result.directories) {
      directorySelect.add(new Option(directory.slice(path ? path.length + 1 : 0), directory));
    }
    document.querySelector("#parent-directory").disabled = !path;
    addButton.disabled = true;
  }

  async function loadLibraries() {
    const result = await requestJson("/api/libraries");
    libraryList.replaceChildren();
    if (result.libraries.length === 0) {
      const empty = document.createElement("p");
      empty.className = "muted";
      empty.textContent = "Nenhuma biblioteca cadastrada.";
      libraryList.append(empty);
      return;
    }
    for (const library of result.libraries) {
      const row = document.createElement("article");
      row.className = "configured-library";
      const details = document.createElement("div");
      details.className = "configured-library-details";
      const name = document.createElement("strong");
      name.textContent = library.name;
      const path = document.createElement("span");
      path.textContent = library.path;
      const kind = document.createElement("span");
      kind.className = `type-pill type-${library.kind}`;
      kind.textContent = library.kind === "anime" ? "Anime" : "Série";
      details.append(name, path);
      const remove = document.createElement("button");
      remove.type = "button";
      remove.className = "button button-quiet";
      remove.textContent = "Remover";
      remove.addEventListener("click", () => removeLibrary(library));
      row.append(details, kind, remove);
      libraryList.append(row);
    }
  }

  async function removeLibrary(library) {
    if (!window.confirm(`Remover a biblioteca "${library.name}"? Os arquivos não serão apagados.`)) return;
    try {
      await requestJson("/api/libraries", {
        method: "DELETE",
        headers: { "Content-Type": "application/json", "X-CSRF-Token": csrfToken },
        body: JSON.stringify({ path: library.path }),
      });
      showFeedback(listFeedback, `Biblioteca "${library.name}" removida. Os arquivos não foram alterados.`);
      await loadLibraries();
    } catch (error) {
      showFeedback(listFeedback, error.message, true);
    }
  }

  directorySelect.addEventListener("change", () => {
    addButton.disabled = !directorySelect.value;
  });
  document.querySelector("#open-directory").addEventListener("click", async () => {
    if (!directorySelect.value) return;
    try {
      await loadDirectories(directorySelect.value);
      showFeedback(feedback, "");
    } catch (error) {
      showFeedback(feedback, error.message, true);
    }
  });
  document.querySelector("#parent-directory").addEventListener("click", async () => {
    const parent = currentDirectory.split("/").slice(0, -1).join("/");
    try {
      await loadDirectories(parent);
      showFeedback(feedback, "");
    } catch (error) {
      showFeedback(feedback, error.message, true);
    }
  });
  libraryForm.addEventListener("submit", async (event) => {
    event.preventDefault();
    if (!directorySelect.value) {
      showFeedback(feedback, "Selecione uma pasta antes de adicionar.", true);
      return;
    }
    addButton.disabled = true;
    try {
      const library = await requestJson("/api/libraries", {
        method: "POST",
        headers: { "Content-Type": "application/json", "X-CSRF-Token": csrfToken },
        body: JSON.stringify({
          path: directorySelect.value,
          kind: document.querySelector("#library-kind").value,
        }),
      });
      showFeedback(feedback, `Biblioteca "${library.name}" adicionada. Execute uma varredura no painel para atualizar o inventário.`);
      await Promise.all([loadDirectories(currentDirectory), loadLibraries()]);
    } catch (error) {
      showFeedback(feedback, error.message, true);
      addButton.disabled = !directorySelect.value;
    }
  });

  Promise.all([loadDirectories(), loadLibraries()]).catch((error) => {
    showFeedback(feedback, `Não foi possível carregar bibliotecas e pastas: ${error.message}`, true);
  });
})();
