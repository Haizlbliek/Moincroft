using System.Text.Json.Nodes;
using Moincroft.Definitions.Models;
using Silk.NET.SDL;

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

	private static BlockStateItem[] ParseBlockStateItems(JsonNode node) {
		BlockStateItem[] items;

		if (node is JsonArray arrayNode) {
			items = arrayNode
				.Select(LoadItem)
				.Where(item => item != null)
				.ToArray()!;
		} else {
			BlockStateItem? singleItem = LoadItem(node);
			items = singleItem != null ? [singleItem] : [];
		}

		return items;
	}

	private static VariantBlockStateData ParseVariants(JsonObject obj) {
		VariantBlockStateData variant = new VariantBlockStateData();

		foreach (KeyValuePair<string, JsonNode?> property in obj) {
			string variantKey = property.Key;
			JsonNode? valueNode = property.Value;

			if (valueNode == null) continue;

			BlockStateItem[] items = ParseBlockStateItems(valueNode);
			if (string.IsNullOrWhiteSpace(variantKey)) {
				variant.DefaultVariant = items;
			}
			else {
				variant.Variants[new PropertyStateKey(variantKey)] = items;
			}
		}

		return variant;
	}

	private static Dictionary<string, string[]> ParseMultiPartCondition(JsonObject obj) {
		Dictionary<string, string[]> condition = [];

		foreach (KeyValuePair<string, JsonNode?> item in obj) {
			condition[item.Key] = item.Value!.GetValue<string>().Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
		}

		return condition;
	}

	private static MultiPartBlockStateData ParseMultiPart(JsonArray arr) {
		MultiPartBlockStateData multiPart = new MultiPartBlockStateData();

		foreach (JsonNode? part in arr) {
			if (part is not JsonObject partObj)
				continue;

			if (!partObj.TryGetPropertyValue("apply", out JsonNode? applyNode) || applyNode == null) {
				continue;
			}

			if (!partObj.TryGetPropertyValue("when", out JsonNode? whenNode) || whenNode is not JsonObject whenObj) {
				continue;
			}

			BlockStateItem[] items = ParseBlockStateItems(applyNode);
			List<Dictionary<string, string[]>> conditions = [];
			MultiPartBlockStateData.Part.ConditionType conditionType;

			if (whenObj.TryGetPropertyValue("OR", out JsonNode? orNode) && orNode is JsonArray orArr) {
				conditionType = MultiPartBlockStateData.Part.ConditionType.Or;
				foreach (JsonNode? conditionNode in orArr) {
					if (conditionNode is not JsonObject conditionObj)
						continue;

					conditions.Add(ParseMultiPartCondition(conditionObj));
				}
			}
			else if (whenObj.TryGetPropertyValue("AND", out JsonNode? andNode) && andNode is JsonArray andArr) {
				conditionType = MultiPartBlockStateData.Part.ConditionType.And;
				foreach (JsonNode? conditionNode in andArr) {
					if (conditionNode is not JsonObject conditionObj)
						continue;

					conditions.Add(ParseMultiPartCondition(conditionObj));
				}
			}
			else {
				conditionType = MultiPartBlockStateData.Part.ConditionType.And;
				conditions.Add(ParseMultiPartCondition(whenObj));
			}

			multiPart.parts.Add(new MultiPartBlockStateData.Part(items, [.. conditions], conditionType));
		}

		return multiPart;
	}

	private static BlockStateData LoadBlockState(string path) {
		JsonObject rootNode = JsonNode.Parse(File.ReadAllText(path))!.AsObject();

		BlockStateData stateData = null!;

		if (rootNode.TryGetPropertyValue("variants", out JsonNode? variantsNode) && variantsNode is JsonObject variantsObj) {
			stateData = ParseVariants(variantsObj);
		}
		else if (rootNode.TryGetPropertyValue("multipart", out JsonNode? multipartNode) && multipartNode is JsonArray multipartArr) {
			stateData = ParseMultiPart(multipartArr);
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