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
					faces = [],
					lightEmisson = -1,
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

				if (elementObj.TryGetPropertyValue("light_emission", out JsonNode? lightEmissonNode) && lightEmissonNode != null) {
					element.lightEmisson = (int) lightEmissonNode;
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

						MiddleModel.Element.Face face = new MiddleModel.Element.Face() {
							texture = faceObj.TryGetPropertyValue("texture", out JsonNode? texNode) ? texNode!.ToString() : "",
							cullface = faceObj.TryGetPropertyValue("cullface", out JsonNode? cullNode) ? cullNode!.ToString() : null,
							rotation = faceObj.TryGetPropertyValue("rotation", out JsonNode? faceRotationNode) && faceRotationNode != null ? (int) faceRotationNode / 90 : 0,
							tintIndex = faceObj.TryGetPropertyValue("tintindex", out JsonNode? tintIndexNode) && tintIndexNode != null ? (int) tintIndexNode : -1,
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

	public static Vector3 Rotate(Vector3 v, Vector3 rotation) {
		float rx = MathF.PI / 180f * rotation.x;
		float ry = MathF.PI / 180f * rotation.y;
		float rz = MathF.PI / 180f * rotation.z;

		float cosX = MathF.Cos(rx);
		float sinX = MathF.Sin(rx);

		float y1 = v.y * cosX - v.z * sinX;
		float z1 = v.y * sinX + v.z * cosX;
		v.y = y1;
		v.z = z1;

		float cosY = MathF.Cos(ry);
		float sinY = MathF.Sin(ry);

		float x2 = v.x * cosY + v.z * sinY;
		float z2 = -v.x * sinY + v.z * cosY;
		v.x = x2;
		v.z = z2;

		float cosZ = MathF.Cos(rz);
		float sinZ = MathF.Sin(rz);

		float x3 = v.x * cosZ - v.y * sinZ;
		float y3 = v.x * sinZ + v.y * cosZ;
		v.x = x3;
		v.y = y3;

		return v;
	}

	private static void ParseMiddleModels() {
		foreach (KeyValuePair<string, MiddleModel> pair in middleModels) {
			string modelKey = pair.Key;
			MiddleModel middleModel = pair.Value;

			List<Model.Quad> quads = [];
			bool skip = false;

			foreach (MiddleModel.Element element in middleModel.elements) {
				foreach (KeyValuePair<string, MiddleModel.Element.Face> face in element.faces) {
					Model.Quad quad = new Model.Quad() {
						direction = DirectionFromFace(face.Key),
						cullFace = face.Value.cullface == null ? Direction.None : DirectionFromFace(face.Value.cullface),
						tintIndex = face.Value.tintIndex,
						lightEmisson = element.lightEmisson,
					};

					FaceBasis faceBasis = Preload.FaceBases[(int) quad.direction];
					Vector3 center = (element.to + element.from) / 16f / 2f;
					Vector3 size = (element.from - element.to) / 16f / 2f;
					size.x = Mathf.Abs(size.x);
					size.y = Mathf.Abs(size.y);
					size.z = Mathf.Abs(size.z);

					Vector3 front = center + size * (Vector3) faceBasis.Front;
					Vector3 rotationOrigin = element.rotationOrigin;
					Vector3 localScale = Vector3.One;

					if (element.rotationRescale) {
						float angleRadX = element.rotation.x * Mathf.Deg2Rad;
						float cosX = Mathf.Cos(angleRadX);
						float scaleFactorX = Mathf.Abs(cosX) < 0.0001f ? 1f : 1f / cosX;

						float angleRadY = element.rotation.y * Mathf.Deg2Rad;
						float cosY = Mathf.Cos(angleRadY);
						float scaleFactorY = Mathf.Abs(cosY) < 0.0001f ? 1f : 1f / cosY;

						float angleRadZ = element.rotation.z * Mathf.Deg2Rad;
						float cosZ = Mathf.Cos(angleRadZ);
						float scaleFactorZ = Mathf.Abs(cosZ) < 0.0001f ? 1f : 1f / cosZ;

						localScale.y *= scaleFactorX; localScale.z *= scaleFactorX;
						localScale.x *= scaleFactorY; localScale.z *= scaleFactorY;
						localScale.x *= scaleFactorZ; localScale.y *= scaleFactorZ;
					}

					Vector3 v0Local = front + size * (Vector3) (-faceBasis.Right + faceBasis.Up) - rotationOrigin;
					Vector3 v1Local = front + size * (Vector3) (faceBasis.Right + faceBasis.Up) - rotationOrigin;
					Vector3 v2Local = front + size * (Vector3) (-faceBasis.Right - faceBasis.Up) - rotationOrigin;
					Vector3 v3Local = front + size * (Vector3) (faceBasis.Right - faceBasis.Up) - rotationOrigin;

					v0Local *= localScale;
					v1Local *= localScale;
					v2Local *= localScale;
					v3Local *= localScale;

					quad.v0 = Rotate(v0Local, element.rotation) + rotationOrigin;
					quad.v1 = Rotate(v1Local, element.rotation) + rotationOrigin;
					quad.v2 = Rotate(v2Local, element.rotation) + rotationOrigin;
					quad.v3 = Rotate(v3Local, element.rotation) + rotationOrigin;

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
						// TODO: turn into pink/black checker
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
					lightEmisson = element.lightEmisson,
					faces = [],
				};

				foreach (KeyValuePair<string, Element.Face> faceKvp in element.faces) {
					clonedElement.faces[faceKvp.Key] = new Element.Face {
						uv = faceKvp.Value.uv == null ? null : (float[])faceKvp.Value.uv.Clone(),
						texture = faceKvp.Value.texture,
						cullface = faceKvp.Value.cullface,
						rotation = faceKvp.Value.rotation,
						tintIndex = faceKvp.Value.tintIndex,
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
			public int lightEmisson;
			public Dictionary<string, Face> faces;

			public struct Face {
				public float[]? uv;
				public string texture;
				public string? cullface;
				public int rotation;
				public int tintIndex;
			}
		}
	}
}