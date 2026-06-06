using System.IO.Compression;
using System.Text;
using StbImageSharp;
using StbImageWriteSharp;
using Stride.Core.Extensions;

namespace Moincroft.Assets;

public static class Assets {
	public static void ExtractAssetsFromJar(string jarPath) {
		string extractPath = "tmp";
		if (Directory.Exists(extractPath)) Directory.Delete(extractPath, true);
		Directory.CreateDirectory(extractPath);

		Console.WriteLine("Extracting assets from JAR...");

		using ZipArchive archive = ZipFile.OpenRead(jarPath);

		foreach (ZipArchiveEntry entry in archive.Entries) {
			if (!entry.FullName.StartsWith("assets")) continue;
			if (string.IsNullOrEmpty(entry.Name)) continue;

			string dest = Path.Combine(extractPath, entry.FullName);
			string? dir = Path.GetDirectoryName(dest);

			if (dir != null) Directory.CreateDirectory(dir);
			entry.ExtractToFile(dest, true);
		}
	}

	private struct Rect(int x, int y, int w, int h) {
		public int X = x, Y = y, W = w, H = h;
	}

	public static void Pack(string[] files, string outputPath) {
		if (files.Length == 0) return;

		var textures = files.Select(f => {
			using FileStream stream = File.OpenRead(f);
			ImageInfo? info = ImageInfo.FromStream(stream);
			return info == null ? null : (new { Path = f, Name = Path.GetFileNameWithoutExtension(f), info.Value.Width, info.Value.Height });
		}).NotNull().OrderByDescending(t => t.Height).ThenByDescending(t => t.Width).ToList();

		long totalArea = textures.Sum(t => (long) t.Width * t.Height);
		int guessWidth = (int) Math.Pow(2, Math.Floor(Math.Log2(Math.Sqrt(totalArea))));
		int guessHeight = (int) Math.Pow(2, Math.Ceiling(Math.Log2(Math.Sqrt(totalArea))));

		List<Rect> freeNodes = freeNodes = [ new Rect(0, 0, guessWidth, guessHeight) ];
		List<(string name, string path, int x, int y, int w, int h)> placements = [];

		foreach (var tex in textures) {
			int nodeIndex = -1;
			for (int i = 0; i < freeNodes.Count; i++) {
				if (tex.Width <= freeNodes[i].W && tex.Height <= freeNodes[i].H) {
					nodeIndex = i;
					break;
				}
			}

			while (nodeIndex == -1) {
				if (guessWidth < guessHeight) {
					freeNodes.Add(new Rect(guessWidth, 0, guessWidth, guessHeight));
					guessWidth *= 2;
				} else {
					freeNodes.Add(new Rect(0, guessHeight, guessWidth, guessHeight));
					guessHeight *= 2;
				}

				for (int i = 0; i < freeNodes.Count; i++) {
					if (tex.Width <= freeNodes[i].W && tex.Height <= freeNodes[i].H) {
						nodeIndex = i;
						break;
					}
				}
			}

			Rect node = freeNodes[nodeIndex];
			freeNodes.RemoveAt(nodeIndex);

			placements.Add((tex.Name, tex.Path, node.X, node.Y, tex.Width, tex.Height));

			if (node.H - tex.Height > 0) {
				freeNodes.Add(new Rect(node.X, node.Y + tex.Height, node.W, node.H - tex.Height));
			}

			if (node.W - tex.Width > 0) {
				freeNodes.Add(new Rect(node.X + tex.Width, node.Y, node.W - tex.Width, tex.Height));
			}

			freeNodes = [.. freeNodes.OrderBy(n => n.W * n.H)];
		}

		int totalWidth = placements.Max(p => p.x + p.w);
		int totalHeight = placements.Max(p => p.y + p.h);

		byte[] atlasData = new byte[totalWidth * totalHeight * 4];
		StringBuilder txtData = new StringBuilder();

		foreach ((string? name, string? path, int x, int y, int width, int height) in placements) {
			using FileStream stream = File.OpenRead(path);
			ImageResult image = ImageResult.FromStream(stream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);

			for (int row = 0; row < height; row++) {
				int sourceOffset = row * width * 4;
				int targetOffset = ((y + row) * totalWidth + x) * 4;
				Array.Copy(image.Data, sourceOffset, atlasData, targetOffset, width * 4);
			}

			txtData.AppendLine($"{name} {x} {y} {width} {height}");
		}

		using (FileStream stream = File.Create(outputPath + ".png")) {
			ImageWriter writer = new ImageWriter();
			writer.WritePng(atlasData, totalWidth, totalHeight, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
		}

		File.WriteAllText(outputPath + ".txt", txtData.ToString());
	}

	public static void BuildTextureAtlas() {
		string rootAssetDir = "./tmp/assets/minecraft/";
		string texturesDir = Path.Combine(rootAssetDir, "textures");
		if (Directory.Exists("assets/generated")) Directory.Delete("assets/generated", true);
		Directory.CreateDirectory("assets/generated");

		string[] blocks = Directory.GetFiles(Path.Combine(texturesDir, "block"), "*.png");
		string[] items = Directory.GetFiles(Path.Combine(texturesDir, "item"), "*.png");

		Pack(blocks, "assets/generated/blocks");
		Pack(items, "assets/generated/items");

		string blockstatesSourceDir = Path.Combine(rootAssetDir, "blockstates/");
		string blockstatesDestDir = "assets/generated/blockstates/";
		CopyJsonFolder(blockstatesSourceDir, blockstatesDestDir);

		string blockModelsSourceDir = Path.Combine(rootAssetDir, "models/block/");
		string blockModelsDestDir = "assets/generated/models/block/";
		CopyJsonFolder(blockModelsSourceDir, blockModelsDestDir);
	}

	private static void CopyJsonFolder(string sourceDir, string destDir) {
		if (!Directory.Exists(sourceDir)) return;
		Directory.CreateDirectory(destDir);

		string[] files = Directory.GetFiles(sourceDir, "*.json");
		foreach (string file in files) {
			string fileName = Path.GetFileName(file);
			string destFile = Path.Combine(destDir, fileName);
			File.Copy(file, destFile, true);
		}
	}
}