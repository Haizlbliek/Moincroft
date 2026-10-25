using Stride.Core.Extensions;

namespace Moincroft.World;

public static class ChunkSerializer {
	public static void SaveChunk(Chunk chunk, string path) {
		using StreamWriter writer = new StreamWriter(path);

		writer.Write($"1 {chunk.cx} {chunk.cy} {chunk.cz}\n");

		BlockType[] palette = chunk.blocks.palette;
		for (int i = 0; i < palette.Length; i++) {
			writer.Write(BlockRegistry.GetBlock(palette[i].Type).data.Id);
			if (!palette[i].State.Empty) {
				writer.Write('[');
				writer.Write(palette[i].State.PropertyKey);
				writer.Write(']');
			}
			writer.Write('\n');
		}
		writer.Write('\n');
		writer.Flush();

		using BinaryWriter bw = new BinaryWriter(writer.BaseStream, System.Text.Encoding.UTF8, leaveOpen: true);
		bw.Write(chunk.blocks.data.Length);

		foreach (ulong x in chunk.blocks.data)
			bw.Write(x);
	}

	private static string ReadLine(BinaryReader reader) {
		List<byte> bytes = [];

		while (reader.BaseStream.Position < reader.BaseStream.Length) {
			byte b = reader.ReadByte();

			if (b == '\n')
				break;

			if (b != '\r')
				bytes.Add(b);
		}

		return System.Text.Encoding.UTF8.GetString([.. bytes]);
	}

	public static void LoadChunk(World world, string path) {
		using FileStream file = File.OpenRead(path);
		using BinaryReader reader = new BinaryReader(file);
		string? header = ReadLine(reader);

		string[] pos = header.Split(' ');

		if (pos[0] != "1")
			throw new Exception($"Unknown version {pos[0]}");

		int cx = int.Parse(pos[1]);
		int cy = int.Parse(pos[2]);
		int cz = int.Parse(pos[3]);

		Chunk chunk = new Chunk(world, cx, cy, cz);

		List<BlockType> palette = [];

		while (true) {
			string? line = ReadLine(reader);

			if (line.Length == 0)
				break;

			BlockId id;
			BlockStateId state = 0;

			int idx = line.IndexOf('[');

			if (idx != -1) {
				string name = line[..idx];
				string[] propertyData = line[(idx + 1)..^1].Split(',');

				id = BlockRegistry.GetBlockId(name);
				Block block = BlockRegistry.GetBlock(id);

				foreach (string propertyString in propertyData) {
					int equals = propertyString.IndexOf('=');

					string propertyName = propertyString[..equals];

					IProperty? property =
						block.Properties.FirstOrDefault(x => x.Name == propertyName);

					if (property == null)
						continue;

					state = block.ModifyState(
						state,
						property,
						property.GetIndexFrom(propertyString[(equals + 1)..])
					);
				}
			}
			else {
				id = BlockRegistry.GetBlockId(line);
			}

			palette.Add(new BlockType(id, state));
		}

		ulong[] data = new ulong[reader.ReadInt32()];

		for (int i = 0; i < data.Length; i++)
			data[i] = reader.ReadUInt64();

		chunk.blocks = new PalettedContainer<BlockType>([.. palette], data);
		world.chunks.Add((chunk.cx, chunk.cy, chunk.cz), chunk);
		chunk.loadState = Chunk.LoadState.Data;
		chunk.QueueRefresh();
	}
}