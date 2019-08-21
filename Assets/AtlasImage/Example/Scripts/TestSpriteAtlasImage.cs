using Dot.Core.UI;
using UnityEngine;
using UnityEngine.U2D;

public class TestSpriteAtlasImage : MonoBehaviour
{
    public SpriteAtlasImage atlasImage;

    private string otherAtlasPath = "ArtRes/Atlas/Icon-2";
    private string spriteNameInOtherAtlas = "icon_home";
    private string spriteNameInSameAtlas = "icon_guojia";

    void OnGUI()
    {
        if(GUILayout.Button("Change SpriteName"))
        {
            atlasImage.SpriteName = spriteNameInSameAtlas;
            atlasImage.SetNativeSize();
        }

        if(GUILayout.Button("Change Atlas And SpriteName"))
        {
            atlasImage.Atlas = Resources.Load<SpriteAtlas>(otherAtlasPath);
            atlasImage.SpriteName = spriteNameInOtherAtlas;
            atlasImage.SetNativeSize();
        }
    }
}
