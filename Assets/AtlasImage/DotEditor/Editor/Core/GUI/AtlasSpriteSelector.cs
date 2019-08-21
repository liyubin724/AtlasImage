using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;
using UnityObject = UnityEngine.Object;

namespace DotEditor.Core.EGUI
{
    public delegate void OnSpriteSelected(string spriteName);

    public class AtlasSpriteSelector : ScriptableWizard
    {
        static public AtlasSpriteSelector spriteSelector;

        public static void Show(SpriteAtlas spriteAtlas,string defaultSpriteName, OnSpriteSelected callback)
        {
            if (spriteSelector != null)
            {
                spriteSelector.Close();
                spriteSelector = null;
            }
            AtlasSpriteSelector selector = ScriptableWizard.DisplayWizard<AtlasSpriteSelector>("Select a Sprite");
            selector.atlas = spriteAtlas;
            selector.mSelectedSpriteName = defaultSpriteName;
            selector.mSpriteSeletectedCallback = callback;
        }
        private SpriteAtlas atlas = null;
        private Sprite mSelectedSprite;
        private string mSelectedSpriteName = "";
        private OnSpriteSelected mSpriteSeletectedCallback;

        private Vector2 mScrollPos = Vector2.zero;
        private float mClickTime = 0f;
        private string mSearchText = "";


        void OnEnable() { spriteSelector = this; }
        void OnDisable() { spriteSelector = null; }
        
        static Texture2D mBackdropTex;

        
        void OnGUI()
        {
            EditorGUIUtility.labelWidth = 80f;

            if (atlas == null)
            {
                GUILayout.Label("No Atlas selected.");
            }
            else
            {
                bool close = false;
                GUILayout.Label(atlas.name + " Sprites", "LODLevelNotifyText");
                GUILayout.Space(12f);

                if (Event.current.type == EventType.Repaint)
                {
                    GUI.color = new Color(0f, 0f, 0f, 0.25f);
                    GUI.DrawTexture(new Rect(0f, GUILayoutUtility.GetLastRect().yMin + 6f, Screen.width, 4f), blankTexture);
                    GUI.DrawTexture(new Rect(0f, GUILayoutUtility.GetLastRect().yMin + 6f, Screen.width, 1f), blankTexture);
                    GUI.DrawTexture(new Rect(0f, GUILayoutUtility.GetLastRect().yMin + 9f, Screen.width, 1f), blankTexture);
                    GUI.color = Color.white;
                }

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(84f);

                    mSearchText = EditorGUILayout.TextField("", mSearchText, "SearchTextField");
                    if (GUILayout.Button("", "SearchCancelButton", GUILayout.Width(18f)))
                    {
                        mSearchText = "";
                        GUIUtility.keyboardControl = 0;
                    }
                    GUILayout.Space(84f);
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(10f);

                Sprite[] sprites = new Sprite[atlas.spriteCount];
                atlas.GetSprites(sprites);
                if (!string.IsNullOrEmpty(mSearchText))
                {
                    List<Sprite> searchSprites = new List<Sprite>();
                    Array.ForEach(sprites, (sprite) =>
                    {
                        if(sprite.name.IndexOf(mSearchText)>=0)
                        {
                            searchSprites.Add(sprite);
                        }
                    });
                    sprites = searchSprites.ToArray();
                }
                
                float size = 80f;
                float padded = size + 10f;
                int columns = Mathf.FloorToInt(Screen.width / padded);
                if (columns < 1) columns = 1;

                int offset = 0;
                Rect rect = new Rect(10f, 0, size, size);

                mScrollPos = GUILayout.BeginScrollView(mScrollPos);
                {
                    int rows = 1;
                    while (offset < sprites.Length)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            int col = 0;
                            rect.x = 10f;

                            for (; offset < sprites.Length; ++offset)
                            {
                                Sprite sprite = sprites[offset];
                                if (sprite == null) continue;
                                string spriteName = sprite.name.Replace("(Clone)", "");

                                if (GUI.Button(rect, ""))
                                {
                                    if (Event.current.button == 0)
                                    {
                                        float delta = Time.realtimeSinceStartup - mClickTime;
                                        mClickTime = Time.realtimeSinceStartup;

                                        if (mSelectedSpriteName != spriteName)
                                        {
                                            if (mSelectedSprite != null)
                                            {
                                                RegisterUndo("Atlas Selection", mSelectedSprite);
                                            }

                                            mSelectedSprite = sprite;
                                            mSelectedSpriteName = spriteName;

                                            Repaint();

                                            mSpriteSeletectedCallback?.Invoke(spriteName);
                                        }
                                        else if (delta < 0.5f) close = true;
                                    }
                                }

                                if (Event.current.type == EventType.Repaint)
                                {
                                    // On top of the button we have a checkboard grid
                                    DrawTiledTexture(rect, backdropTexture);
                                    Rect uv = new Rect(sprite.rect.x, sprite.rect.y, sprite.rect.width, sprite.rect.height);
                                    uv = ConvertToTexCoords(uv, (int)sprite.texture.width, (int)sprite.texture.height);

                                    // Calculate the texture's scale that's needed to display the sprite in the clipped area
                                    float scaleX = rect.width / uv.width;
                                    float scaleY = rect.height / uv.height;

                                    // Stretch the sprite so that it will appear proper
                                    float aspect = (scaleY / scaleX) / ((float)sprite.texture.height / sprite.texture.width);
                                    Rect clipRect = rect;

                                    if (aspect != 1f)
                                    {
                                        if (aspect < 1f)
                                        {
                                            // The sprite is taller than it is wider
                                            float padding = size * (1f - aspect) * 0.5f;
                                            clipRect.xMin += padding;
                                            clipRect.xMax -= padding;
                                        }
                                        else
                                        {
                                            // The sprite is wider than it is taller
                                            float padding = size * (1f - 1f / aspect) * 0.5f;
                                            clipRect.yMin += padding;
                                            clipRect.yMax -= padding;
                                        }
                                    }
                                    SpriteDrawUtility.DrawSprite(sprite, clipRect, GUI.color);
                                    // Draw the selection
                                    if (mSelectedSpriteName == spriteName)
                                    {
                                        DrawOutline(rect, new Color(0.4f, 1f, 0f, 1f));
                                    }
                                }

                                GUI.backgroundColor = new Color(1f, 1f, 1f, 0.5f);
                                GUI.contentColor = new Color(1f, 1f, 1f, 0.7f);
                                GUI.Label(new Rect(rect.x, rect.y + rect.height, rect.width, 32f), spriteName, "ProgressBarBack");
                                GUI.contentColor = Color.white;
                                GUI.backgroundColor = Color.white;
                                col++;
                                if (col >= columns)
                                {
                                    ++offset;
                                    break;
                                }
                                rect.x += padded;
                            }
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.Space(padded);
                        rect.y += padded + 26;
                        ++rows;
                    }
                    GUILayout.Space(rows * 26);
                }
                GUILayout.EndScrollView();

                if (close) Close();
            }
        }
        
        static public void RegisterUndo(string name, params UnityObject[] objects)
        {
            if (objects != null && objects.Length > 0)
            {
                UnityEditor.Undo.RecordObjects(objects, name);

                foreach (UnityObject obj in objects)
                {
                    if (obj == null) continue;
                    EditorUtility.SetDirty(obj);
                }
            }
        }

        static public void DrawTiledTexture(Rect rect, Texture tex)
        {
            GUI.BeginGroup(rect);
            {
                int width = Mathf.RoundToInt(rect.width);
                int height = Mathf.RoundToInt(rect.height);

                for (int y = 0; y < height; y += tex.height)
                {
                    for (int x = 0; x < width; x += tex.width)
                    {
                        GUI.DrawTexture(new Rect(x, y, tex.width, tex.height), tex);
                    }
                }
            }
            GUI.EndGroup();
        }

        static public Rect ConvertToTexCoords(Rect rect, int width, int height)
        {
            Rect final = rect;

            if (width != 0f && height != 0f)
            {
                final.xMin = rect.xMin / width;
                final.xMax = rect.xMax / width;
                final.yMin = 1f - rect.yMax / height;
                final.yMax = 1f - rect.yMin / height;
            }
            return final;
        }

        static public Texture2D blankTexture
        {
            get
            {
                return EditorGUIUtility.whiteTexture;
            }
        }

        static public Texture2D backdropTexture
        {
            get
            {
                if (mBackdropTex == null) mBackdropTex = CreateCheckerTex(
                    new Color(0.1f, 0.1f, 0.1f, 0.5f),
                    new Color(0.2f, 0.2f, 0.2f, 0.5f));
                return mBackdropTex;
            }
        }


        static Texture2D CreateCheckerTex(Color c0, Color c1)
        {
            Texture2D tex = new Texture2D(16, 16);
            tex.name = "[Generated] Checker Texture";
            tex.hideFlags = HideFlags.DontSave;

            for (int y = 0; y < 8; ++y) for (int x = 0; x < 8; ++x) tex.SetPixel(x, y, c1);
            for (int y = 8; y < 16; ++y) for (int x = 0; x < 8; ++x) tex.SetPixel(x, y, c0);
            for (int y = 0; y < 8; ++y) for (int x = 8; x < 16; ++x) tex.SetPixel(x, y, c0);
            for (int y = 8; y < 16; ++y) for (int x = 8; x < 16; ++x) tex.SetPixel(x, y, c1);

            tex.Apply();
            tex.filterMode = FilterMode.Point;
            return tex;
        }

        static public void DrawOutline(Rect rect, Color color)
        {
            if (Event.current.type == EventType.Repaint)
            {
                Texture2D tex = blankTexture;
                GUI.color = color;
                GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, 1f, rect.height), tex);
                GUI.DrawTexture(new Rect(rect.xMax, rect.yMin, 1f, rect.height), tex);
                GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, rect.width, 1f), tex);
                GUI.DrawTexture(new Rect(rect.xMin, rect.yMax, rect.width, 1f), tex);
                GUI.color = Color.white;
            }
        }
    }
}
