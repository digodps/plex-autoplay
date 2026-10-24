import json
import tempfile
import unittest
from pathlib import Path

from plex_autoplay.infrastructure.media.library_repository import LibraryRepository


class TestLibraryRepository(unittest.TestCase):
    def test_libraries_persist_across_repository_instances_and_can_be_removed(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            root = Path(temp_dir) / "media"
            (root / "Anime").mkdir(parents=True)
            config_file = Path(temp_dir) / "config" / "libraries.json"
            repository = LibraryRepository(root, config_file)
            added = repository.add_library("Anime", "anime")

            loaded = LibraryRepository(root, config_file).list_libraries()

            self.assertEqual(added.path, "Anime")
            self.assertEqual([(item.name, item.kind) for item in loaded], [("Anime", "anime")])
            self.assertTrue(repository.remove_library("Anime"))
            self.assertEqual(repository.list_libraries(), [])
            self.assertTrue(config_file.exists())

    def test_rejects_duplicates_invalid_kind_and_missing_directory(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            root = Path(temp_dir) / "media"
            root.mkdir()
            (root / "TV").mkdir()
            repository = LibraryRepository(root, Path(temp_dir) / "libraries.json")
            repository.add_library("TV", "series")

            with self.assertRaises(ValueError):
                repository.add_library("TV", "anime")
            with self.assertRaises(ValueError):
                repository.add_library("TV", "movies")
            with self.assertRaises(ValueError):
                repository.add_library("missing", "series")

    def test_writes_json_atomically_with_private_permissions(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            root = Path(temp_dir) / "media"
            (root / "TV").mkdir(parents=True)
            config_file = Path(temp_dir) / "config" / "libraries.json"
            LibraryRepository(root, config_file).add_library("TV", "series")

            self.assertEqual(json.loads(config_file.read_text(encoding="utf-8"))[0]["path"], "TV")
            self.assertEqual(config_file.stat().st_mode & 0o777, 0o600)
            self.assertEqual(list(config_file.parent.iterdir()), [config_file])

    def test_directory_listing_is_limited_to_existing_subdirectories(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            root = Path(temp_dir) / "media"
            (root / "TV" / "Series").mkdir(parents=True)
            (root / "notes.txt").write_text("not a folder", encoding="utf-8")
            repository = LibraryRepository(root, Path(temp_dir) / "libraries.json")

            self.assertEqual(repository.list_directories(), ["TV"])
            self.assertEqual(repository.list_directories("TV"), ["TV/Series"])
            with self.assertRaises(ValueError):
                repository.list_directories("../outside")
            with self.assertRaises(ValueError):
                repository.list_directories("notes.txt")

    def test_rejects_external_symlinks_when_listing_or_adding(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            root = Path(temp_dir) / "media"
            outside = Path(temp_dir) / "outside"
            root.mkdir()
            outside.mkdir()
            (root / "External").symlink_to(outside, target_is_directory=True)
            repository = LibraryRepository(root, Path(temp_dir) / "libraries.json")

            self.assertEqual(repository.list_directories(), [])
            with self.assertRaises(ValueError):
                repository.add_library("External", "series")


if __name__ == "__main__":
    unittest.main()
