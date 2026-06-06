using System.Text.Json.Nodes;
using Moincroft.Definitions.Models;

namespace Moincroft.Definitions;

public static class BlockStateLoader {
	private static readonly Dictionary<string, BlockStateData> blockStates = [];

	private static readonly string blockStatesDir = Path.Combine("assets", "generated", "blockstates");

	private static BlockStateItem? LoadItem(JsonNode? node) {
		JsonObject? obj = node?.AsObject();
		if (obj == null)
			return null;

		if (!obj.TryGetPropertyValue("model", out JsonNode? modelNode) || modelNode is not JsonValue modelValue) {
			return null;
		}
		string modelKey = modelValue.ToString();
		if (!modelKey.Contains(':')) modelKey = $"minecraft:{modelKey}";

		BlockStateItem item = new BlockStateItem() { model = ModelLoader.GetModel(modelKey) };

		if (obj.TryGetPropertyValue("x", out JsonNode? xNode) && xNode is JsonValue xValue) {
			item.rotationX = xValue.GetValue<int>() / 90;
		}
		if (obj.TryGetPropertyValue("y", out JsonNode? yNode) && yNode is JsonValue yValue) {
			item.rotationY = yValue.GetValue<int>() / 90;
		}
		if (obj.TryGetPropertyValue("z", out JsonNode? zNode) && zNode is JsonValue zValue) {
			item.rotationZ = zValue.GetValue<int>() / 90;
		}
		if (obj.TryGetPropertyValue("weight", out JsonNode? weightNode) && weightNode is JsonValue weightValue) {
			item.weight = weightValue.GetValue<int>();
		}
		if (obj.TryGetPropertyValue("uvlock", out JsonNode? uvLockNode) && uvLockNode is JsonValue uvLockValue) {
			item.UVLock = uvLockValue.GetValue<bool>();
		}

		return item;
	}

	private static BlockStateData LoadBlockState(string path) {
		JsonObject rootNode = JsonNode.Parse(File.ReadAllText(path))!.AsObject();

		BlockStateData stateData = null!;

		if (rootNode.TryGetPropertyValue("variants", out JsonNode? variantsNode) && variantsNode is JsonObject variantsObj) {
			VariantBlockStateData variant = new VariantBlockStateData();
			stateData = variant;

			foreach (KeyValuePair<string, JsonNode?> property in variantsObj) {
				string variantKey = property.Key;
				JsonNode? valueNode = property.Value;

				if (valueNode == null) continue;

				BlockStateItem[] items;

				if (valueNode is JsonArray arrayNode) {
					items = arrayNode
						.Select(node => LoadItem(node))
						.Where(item => item != null)
						.ToArray()!;
				} else {
					BlockStateItem? singleItem = LoadItem(valueNode);
					items = singleItem != null ? [singleItem] : [];
				}

				if (string.IsNullOrWhiteSpace(variantKey)) {
					variant.DefaultVariant = items;
				}
				else {
					variant.Variants[new PropertyStateKey(variantKey)] = items;
				}
			}
		}

		if (stateData == null) throw new Exception($"Invalid blockstate: {path}");

		return stateData;
	}

	public static void Initialize() {
		foreach (string filePath in Directory.GetFiles(blockStatesDir)) {
			string id = Path.GetFileNameWithoutExtension(filePath);
			blockStates[id] = LoadBlockState(filePath);
		}
	}

	public static BlockStateData GetBlockStateData(string id) => blockStates[id];
}