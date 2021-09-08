using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class sliceSprite : MonoBehaviour {

	[MenuItem("Assets/Slice PNG")]
	static void SetSpriteNames()
	{
		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		int index = path.LastIndexOf (".");
		string ext = path.Substring (index+1);

		if (index > 0 && ext.Equals ("xml")) {
			XmlDocument xDoc = new XmlDocument ();
			try{
				xDoc.Load(path);
				XmlElement xRoot = xDoc.DocumentElement;
				float totalNodes = (float)xRoot.SelectNodes("descendant::*").Count;
				float nodesProcessed = 0f;
				float totalProgess = 0f;
				Vector4 transfrmComponents = new Vector4 (0f, 0f, 0f, 0f);
				XmlNodeList textureIteration = xDoc.GetElementsByTagName("texture"); // Iterate through every Image File get its name and create path
				foreach(XmlNode iteratedTexture in textureIteration){
					nodesProcessed++;
					string textureName = iteratedTexture.Attributes["name"].Value;
					totalProgess = ((nodesProcessed/totalNodes) * 100f) / 100f;
					EditorUtility.DisplayProgressBar("Slicing image(s)", "Slicing : " + textureName, totalProgess);
					int pathNameIndex = path.LastIndexOf("/");
					string pathToUse = path.Substring(0, pathNameIndex + 1) + textureName;
					TextureImporterSettings tSettings = new TextureImporterSettings();
					TextureImporter tI = AssetImporter.GetAtPath(pathToUse) as TextureImporter;
					tI.ReadTextureSettings(tSettings);
					tSettings.spriteAlignment = (int)SpriteAlignment.Custom;
					tSettings.mipmapEnabled = false;
					tI.SetTextureSettings(tSettings);
					tI.isReadable = true;
					tI.textureType = TextureImporterType.Sprite;
					tI.spriteImportMode = SpriteImportMode.Multiple;
					tI.spritePixelsPerUnit = 100f;
					tI.maxTextureSize = 4096;
					tI.sRGBTexture = false;
					List<SpriteMetaData> newData = new List<SpriteMetaData>();

					XmlNodeList spriteIteration = iteratedTexture.ChildNodes; // Iterate through every sprite in image and get its name and transforms
					foreach(XmlNode iteratedSprite in spriteIteration){
						nodesProcessed++;
						totalProgess = ((nodesProcessed/totalNodes) * 100f) / 100f;
						EditorUtility.DisplayProgressBar("Slicing image(s)", "Slicing : " + textureName, totalProgess);
						string spriteName = iteratedSprite.Attributes["name"].Value;

						XmlNodeList spriteTransforms = iteratedSprite.ChildNodes; // Iterate through every transform component of sprite and save them
						foreach(XmlNode spriteInfo in spriteTransforms){
							nodesProcessed++;
							totalProgess = ((nodesProcessed/totalNodes) * 100f) / 100f;
							EditorUtility.DisplayProgressBar("Slicing image(s)", "Slicing : " + textureName, totalProgess);
							float res;
							switch(spriteInfo.Name){
							case "x":
								float.TryParse(spriteInfo.InnerText, out res);
								transfrmComponents.x = res;
								break;
							case "y":
								float.TryParse(spriteInfo.InnerText, out res);
								transfrmComponents.y = res;
								break;
							case "width":
								float.TryParse(spriteInfo.InnerText, out res);
								transfrmComponents.z = res;
								break;
							case "height":
								float.TryParse(spriteInfo.InnerText, out res);
								transfrmComponents.w = res;
								break;
							}
						}
					SpriteMetaData smd = new SpriteMetaData();
					smd.pivot = new Vector2(0.5f, 0.5f);
					smd.alignment = 9;
					smd.name = spriteName;
					smd.rect = new Rect(transfrmComponents.x, transfrmComponents.y, transfrmComponents.z, transfrmComponents.w);
					newData.Add(smd);
					}
					tI.spritesheet = newData.ToArray();
					AssetDatabase.ImportAsset(pathToUse, ImportAssetOptions.Default);
				}
				EditorUtility.ClearProgressBar();
				
			}catch(XmlException xEx){
				if (EditorUtility.DisplayDialog ("Asset Load Failed", "Could not Load XML file : \n\n" + xEx.Message, "Ok"))
					return;
				else
					return;
			}
		} else if (EditorUtility.DisplayDialog ("Unexpected Asset", "Need XML file to proceed", "Ok"))
			return;
		else
			return;
	}

	[MenuItem("Assets/Slice PNG", true)]
	private static bool ifPNG(){
		return Selection.activeObject.GetType() == typeof(TextAsset);
	}
}
