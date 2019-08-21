using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Dot.Core.UI
{
    [AddComponentMenu("UI/Atlas Image", 10)]
    public class SpriteAtlasImage : Image
    {
        [SerializeField]
        private SpriteAtlas m_SpriteAtlas;
        public SpriteAtlas Atlas
        {
            get
            {
                return m_SpriteAtlas;
            }
            set
            {
                if(m_SpriteAtlas !=value)
                {
                    m_SpriteAtlas = value;
                    ChangeSprite();
                }
            }
        }

        [SerializeField]
        private string m_SpriteName = "";
        public string SpriteName
        {
            get
            {
                return m_SpriteName;
            }
            set
            {
                if(m_SpriteName !=value)
                {
                    m_SpriteName = value;
                    ChangeSprite();
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();
            if(sprite == null)
            {
                ChangeSprite();
            }else
            {
                string sn = sprite.name.Replace("(Clone)", "");
                if(sn != SpriteName)
                {
                    ChangeSprite();
                }
            }
        }

        private void ChangeSprite()
        {
            sprite = Atlas ? Atlas.GetSprite(SpriteName) : null;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            ChangeSprite();
            base.OnValidate();
        }
#endif
    }
}


