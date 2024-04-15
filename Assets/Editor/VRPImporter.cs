using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.AssetImporters;

[ScriptedImporter( 1, "vrp" )]
public class VRPImporter : ScriptedImporter {
    public override void OnImportAsset( AssetImportContext ctx ) {
        TextAsset subAsset = new TextAsset( File.ReadAllText( ctx.assetPath ) );
        ctx.AddObjectToAsset( "text", subAsset );
        ctx.SetMainObject( subAsset );
    }
}