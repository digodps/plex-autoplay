# PC Development Build: RTX 4080 for LLM Development
## Brazilian Market Pricing (Oct 2026)

## Overview
A desktop PC built around NVIDIA RTX 4080 for local LLM development, training, and programming without the RAM limitations of Mac Mini M4 Pro (24GB).

---

## Component List & Pricing

| Component | Model | Qty | Price (R$) | Notes |
|---|---|---|---|---|
| **GPU** | NVIDIA RTX 4080 16GB GDDR6X | 1 | ~11,500 | 16GB VRAM for LLMs (Llama 3 8B, 70B offload) |
| **CPU** | AMD Ryzen 9 7950X | 1 | ~5,500 | 16 cores, no-iGPU bottleneck; PBO enabled |
| **Motherboard** | MSI B550 TOMAHAWK MAX WIFI | 1 | ~2,200 | B550 supports Ryzen 7000; WiFi 6E |
| **RAM** | DDR5-6000 32GB (2x16GB) Kit | 2 kits | ~1,800×2 = 3,600 | Total 64GB unified memory (AMD dual-channel) |
| **Storage** | Samsung 990 Pro 1TB NVMe | 1 | ~850 | Gen4 fastest; OS + models + datasets |
| **PSU** | be quiet! Straight Power 11 850W Gold | 1 | ~950 | 850W headroom for RTX 4080 + CPU overclock |
| **CPU Cooler** | be quiet! Pure Rock 2 Rev.2 | 1 | ~230 | Quiet, capable stock |
| **Case** | Fractal Design Meshify 2 (Black/White) | 1 | ~650 | Excellent airflow for RTX 4080 |
| **OS** | Windows 11 Pro / Ubuntu 24.04 | - | - | Dual-boot for CUDA / Python dev |

**Subtotal:** ~R$ 26,480
**Total ~R$ 26,500**

---

## Why This Config for LLM Dev

- **RTX 4080 16GB VRAM**: Runs Llama 3 8B locally with context ~8K tokens. Larger models (13B-70B) can use offload to system RAM (64GB total).
- **64GB System RAM**: Critical. Allows loading models up to ~13B params in main RAM, offload rest to VRAM+system swap. Mac Mini's 24GB severely limits this.
- **Ryzen 9 7950X**: 16 cores / 32 threads speeds up data preprocessing, tokenization, and non-GPU compute. No iGPU confusion.
- **1TB Gen4 SSD**: Fast model loading. Llama 3 8B weights ~5GB, 70B ~40GB—multiple models fit.
- **Linux + Windows dual-boot**: PyTorch/TensorFlow with CUDA 12.x on Windows; native Linux performance for inference scripts.

---

## LLM Performance Estimates (RTX 4080 + 64GB)

| Model | VRAM Usage | Context | Speed (tokens/sec) | Viability |
|---|---|---|---|---|
| Llama 3 8B | ~5GB | 8K | 20-35 tok/s | ✅ Native |
| Llama 3 70B | ~40GB | 8K | 5-8 tok/s | ✅ Offload to 64GB RAM |
| Mistral 7B | ~4GB | 8K | 40-50 tok/s | ✅ Native |
| Nemotron 8B | ~6GB | 8K | 25-35 tok/s | ✅ Native |

---

## Alternative: Lower Budget (~R$ 18,000)

If RTX 4080 is out of reach, consider:
- **RTX 4070 Ti Super 16GB** (~R$ 8,500) + same CPU/RAM/SSD
- Slightly less VRAM headroom but still excellent for 7B-13B models

---

## Quick Validation Script (Python)

```python
import torch
from transformers import AutoModelForCausalLM, AutoTokenizer

# Quick check: can we load Llama 3 8B?
model_id = "meta-llama/Meta-Llama-3-8B-Instruct"
tokenizer = AutoTokenizer.from_pretrained(model_id)
model = AutoModelForCausalLM.from_pretrained(
    model_id, 
    torch_dtype=torch.float16, 
    device_map="auto"
)
print(f"Model loaded on: {model.device}")
print(f"VRAM used: {model.get_memory_footprint() / 1e9:.1f} GB")
```

*Run after build to verify 16GB VRAM sufficiency.*

---

## Shopping Tips (Brazil)

- **GPU**: Check Kabum, Pichau, Amazon Brazil for RTX 4080 models (Galax, MSI, Asus TUF)
- **CPU/Mobo/RAM**: Buy Ryzen 7950X + DDR5 kit as separate components for best value
- **SSD**: 990 Pro or Crucial T700 (PCIe 5.0) if motherboard supports
- **PSU**: 850W Gold minimum—RTX 4080 + Ryzen 9 can peak ~550W combined

---