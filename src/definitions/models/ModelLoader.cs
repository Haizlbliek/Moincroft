using System.Text.Json.Nodes;
using Silk.NET.Input;

namespace Moincroft.Definitions.Models;

public static class ModelLoader {
	private static readonly Dictionary<string, Model> models = [];
	private static readonly Dictionary<string, MiddleModel> middleModels = [];
	private static readonly string modelsDir = Path.Combine("assets", "generated", "models");

	private static MiddleModel GetMiddleModel(string modelName) {
		string modelKey = modelName.StartsWith("minecraft:") ? modelName : $"minecraft:{modelName}";
		modelName = modelName.RemoveStart("minecraft:");

		if (middleModels.TryGetValue(modelKey, out MiddleModel? existingModel))
			return existingModel;

		string filePath = Path.Combine(modelsDir, modelName + ".json");
		JsonObject currentJson = JsonNode.Parse(File.ReadAllText(filePath))!.AsObject();

		MiddleModel newModel = new MiddleModel();

		if (currentJson.TryGetPropertyValue("parent", out JsonNode? parentNode) && parentNode != null) {
			string parentName = parentNode.ToString();
			MiddleModel parentModel = GetMiddleModel(parentName);
			newModel.MergeFrom(parentModel);
		}

		if (currentJson.TryGetPropertyValue("textures", out JsonNode? texturesNode) && texturesNode != null) {
			foreach (KeyValuePair<string, JsonNode?> property in texturesNode.AsObject()) {
				if (property.Value != null) {
					if (property.Value is JsonObject obj) {
						newModel.textures[property.Key] = obj["sprite"]!.GetValue<string>();
					}
					else {
						newModel.textures[property.Key] = property.Value.ToString();
					}
				}
			}
		}
		if (currentJson.TryGetPropertyValue("elements", out JsonNode? elementsNode) && elementsNode != null) {
			newModel.elements.Clear();

			foreach (JsonNode? elementNode in elementsNode.AsArray()) {
				if (elementNode == null) continue;
				JsonObject elementObj = elementNode.AsObject();

				MiddleModel.Element element = new MiddleModel.Element {
					faces = []
				};

				if (elementObj.TryGetPropertyValue("from", out JsonNode? fromNode) && fromNode != null) {
					JsonArray arr = fromNode.AsArray();
					element.from = new Vector3(
						(float)arr[0]!,
						(float)arr[1]!,
						(float)arr[2]!
					);
				}

				if (elementObj.TryGetPropertyValue("to", out JsonNode? toNode) && toNode != null) {
					JsonArray arr = toNode.AsArray();
					element.to = new Vector3(
						(float)arr[0]!,
						(float)arr[1]!,
						(float)arr[2]!
					);
				}

				if (elementObj.TryGetPropertyValue("rotation", out JsonNode? rotationNode) && rotationNode is JsonObject rotationObj) {
					if (rotationObj.TryGetPropertyValue("rescale", out JsonNode? rescaleNode) && rescaleNode != null) {
						element.rotationRescale = rescaleNode.GetValue<bool>();
					}

					if (rotationObj.TryGetPropertyValue("origin", out JsonNode? originNode) && originNode is JsonArray originArr) {
						element.rotationOrigin = new Vector3(((float) originArr[0]!) / 16f, ((float) originArr[1]!) / 16f, ((float) originArr[2]!) / 16f);
					}

					if (rotationObj.TryGetPropertyValue("axis", out JsonNode? axisNode) && axisNode != null
					 && rotationObj.TryGetPropertyValue("angle", out JsonNode? angleNode) && angleNode != null) {
						string axis = (string) axisNode!;
						float angle = (float) angleNode!;
						if (axis == "x") element.rotation = new Vector3(angle, 0f, 0f);
						if (axis == "y") element.rotation = new Vector3(0f, angle, 0f);
						if (axis == "z") element.rotation = new Vector3(0f, 0f, angle);
					}
					else {
						if (rotationObj.TryGetPropertyValue("x", out JsonNode? xNode) && xNode != null) {
							element.rotation.x = (float) xNode!;
						}
						if (rotationObj.TryGetPropertyValue("y", out JsonNode? yNode) && yNode != null) {
							element.rotation.y = (float) yNode!;
						}
						if (rotationObj.TryGetPropertyValue("z", out JsonNode? zNode) && zNode != null) {
							element.rotation.z = (float) zNode!;
						}
					}
				}

				if (elementObj.TryGetPropertyValue("faces", out JsonNode? facesNode) && facesNode != null) {
					foreach (KeyValuePair<string, JsonNode?> faceProperty in facesNode.AsObject()) {
						if (faceProperty.Value == null) continue;

						JsonObject faceObj = faceProperty.Value.AsObject();

						MiddleModel.Element.Face face = new MiddleModel.Element.Face {
							texture = faceObj.TryGetPropertyValue("texture", out JsonNode? texNode) ? texNode!.ToString() : "",
							cullface = faceObj.TryGetPropertyValue("cullface", out JsonNode? cullNode) ? cullNode!.ToString() : null,
							rotation = faceObj.TryGetPropertyValue("rotation", out JsonNode? faceRotationNode) ? ((int) faceRotationNode! / 90) : 0,
						};

						if (faceObj.TryGetPropertyValue("uv", out JsonNode? uvNode) && uvNode != null) {
							JsonArray uvArr = uvNode.AsArray();
							face.uv = [ (float)uvArr[0]!, (float)uvArr[1]!, (float)uvArr[2]!, (float)uvArr[3]! ];
						}

						element.faces[faceProperty.Key] = face;
					}
				}

				newModel.elements.Add(element);
			}
		}

		middleModels[modelKey] = newModel;
		return newModel;
	}

	private static void LoadMiddleModels() {
		foreach (string modelPath in Directory.GetFiles(modelsDir, "*.json", SearchOption.AllDirectories)) {
			GetMiddleModel(Path.ChangeExtension(Path.GetRelativePath(modelsDir, modelPath), null));
		}
	}

	private static Direction DirectionFromFace(string face) => face switch {
		"north" => Direction.North,
		"south" => Direction.South,
		"east" => Direction.East,
		"west" => Direction.West,
		"up" => Direction.Up,
		"down" => Direction.Down,
		_ => Direction.None
	};

	private static void ParseMiddleModels() {
		foreach (KeyValuePair<string, MiddleModel> pair in middleModels) {
			string modelKey = pair.Key;
			MiddleModel middleModel = pair.Value;

			List<Model.Quad> quads = [];
			bool skip = false;

			foreach (MiddleModel.Element element in middleModel.elements) {
				foreach (KeyValuePair<string, MiddleModel.Element.Face> face in element.faces) {
					Model.Quad quad = new Model.Quad {
						direction = DirectionFromFace(face.Key),
						from = element.from,
						to = element.to,
						cullFace = face.Value.cullface == null ? Direction.None : DirectionFromFace(face.Value.cullface),
						rotation = element.rotation,
						rotationOrigin = element.rotationOrigin,
						rotationRescale = element.rotationRescale,
					};
					string texturePath = face.Value.texture.RemoveStart('#');
					while (middleModel.textures.TryGetValue(texturePath, out string? resolved)) {
						texturePath = resolved.RemoveStart('#');
					}
					texturePath = texturePath.RemoveStart("minecraft:").RemoveStart("block/");

					FaceUv faceUv;
					try {
						faceUv = Atlas.GetFace(texturePath);
					}
					catch (Exception) {
						Console.WriteLine($"Parsing: {pair.Key}");
						Console.WriteLine($"WARNING: Missing texture: {texturePath}");
						skip = true;
						break;
					}

					float[]? uv = face.Value.uv;
					if (uv == null || uv.Length == 0) {
						uv = face.Key switch {
							"north" => [16 - element.to.x, 16 - element.to.y, 16 - element.from.x, 16 - element.from.y],
							"south" => [element.from.x, 16 - element.to.y, element.to.x, 16 - element.from.y],
							"west"  => [element.from.z, 16 - element.to.y, element.to.z, 16 - element.from.y],
							"east"  => [16 - element.to.z, 16 - element.to.y, 16 - element.from.z, 16 - element.from.y],
							"up"    => [element.from.x, element.from.z, element.to.x, element.to.z],
							"down"  => [element.from.x, 16 - element.to.z, element.to.x, 16 - element.from.z],
							_       => [0, 0, 16, 16]
						};
					}

					float u0 = uv[0] / Atlas.AtlasWidth + faceUv.X;
					float v0 = uv[1] / Atlas.AtlasHeight + faceUv.Y;
					float u1 = uv[2] / Atlas.AtlasWidth + faceUv.X;
					float v1 = uv[3] / Atlas.AtlasHeight + faceUv.Y;

					switch (face.Value.rotation) {
						case 3:
							quad.uv0 = new Vector2(u1, v1);
							quad.uv1 = new Vector2(u1, v0);
							quad.uv2 = new Vector2(u0, v1);
							quad.uv3 = new Vector2(u0, v0);
							break;

						case 2:
							quad.uv0 = new Vector2(u0, v1);
							quad.uv1 = new Vector2(u1, v1);
							quad.uv2 = new Vector2(u0, v0);
							quad.uv3 = new Vector2(u1, v0);
							break;

						case 1:
							quad.uv0 = new Vector2(u0, v0);
							quad.uv1 = new Vector2(u0, v1);
							quad.uv2 = new Vector2(u1, v0);
							quad.uv3 = new Vector2(u1, v1);
							break;

						default:
							quad.uv0 = new Vector2(u1, v0);
							quad.uv1 = new Vector2(u0, v0);
							quad.uv2 = new Vector2(u1, v1);
							quad.uv3 = new Vector2(u0, v1);
							break;
					}

					quads.Add(quad);
				}

				if (skip)
					break;
			}

			if (!skip) {
				models.Add(modelKey, new Model() { quads = [..quads] });
			}
		}
	}

	public static Model GetModel(string modelKey) {
		return models[modelKey];
	}

	public static void Initialize() {
		LoadMiddleModels();
		ParseMiddleModels();
	}

	private class MiddleModel {
		public readonly Dictionary<string, string> textures = [];
		public readonly List<Element> elements = [];

		public void MergeFrom(MiddleModel other) {
			foreach (KeyValuePair<string, string> kvp in other.textures) {
				this.textures[kvp.Key] = kvp.Value;
			}

			foreach (Element element in other.elements) {
				Element clonedElement = new Element {
					from = element.from,
					to = element.to,
					rotation = element.rotation,
					rotationOrigin = element.rotationOrigin,
					rotationRescale = element.rotationRescale,
					faces = [],
				};

				foreach (KeyValuePair<string, Element.Face> faceKvp in element.faces) {
					clonedElement.faces[faceKvp.Key] = new Element.Face {
						uv = faceKvp.Value.uv == null ? null : (float[])faceKvp.Value.uv.Clone(),
						texture = faceKvp.Value.texture,
						cullface = faceKvp.Value.cullface,
						rotation = faceKvp.Value.rotation,
					};
				}
				this.elements.Add(clonedElement);
			}
		}

		public struct Element {
			public Vector3 from;
			public Vector3 to;
			public Vector3 rotation;
			public Vector3 rotationOrigin;
			public bool rotationRescale;
			public Dictionary<string, Face> faces;

			public struct Face {
				public float[]? uv;
				public string texture;
				public string? cullface;
				public int rotation;
				// LATER REVIEW: tintIndex
			}
		}
	}
}