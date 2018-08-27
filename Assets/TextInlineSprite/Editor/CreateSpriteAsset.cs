using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public static class CreateSpriteAsset
{
    // 创建静态表情
    [MenuItem("Assets/Create/Sprite Asset/Static",false,10)]
    static void CreateAssetStatic()
    {
        CreateAsset(true);
    }

    // 创建动态表情
    [MenuItem("Assets/Create/Sprite Asset/Animation", false, 10)]
    static void CreateAssetAnimation()
    {
        CreateAsset(false);
    }

    static void CreateAsset(bool isStatic)
    {
        Object target = Selection.activeObject;
        if (target == null || target.GetType() != typeof(Texture2D))
            return;

        Texture2D sourceTex = target as Texture2D;
        //整体路径
        string filePathWithName = AssetDatabase.GetAssetPath(sourceTex);
        //带后缀的文件名
        string fileNameWithExtension = Path.GetFileName(filePathWithName);
        //不带后缀的文件名
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePathWithName);
        //不带文件名的路径
        string filePath = filePathWithName.Replace(fileNameWithExtension, "");

        SpriteAsset spriteAsset = AssetDatabase.LoadAssetAtPath(filePath + fileNameWithoutExtension + ".asset", typeof(SpriteAsset)) as SpriteAsset;
        bool isNewAsset = spriteAsset == null ? true : false;
        if (isNewAsset)
        {
            spriteAsset = ScriptableObject.CreateInstance<SpriteAsset>();
            spriteAsset.IsStatic = isStatic;
            spriteAsset.TexSource = sourceTex;
            spriteAsset.ListSpriteGroup = GetAssetSpriteInfor(sourceTex, isStatic);
            AssetDatabase.CreateAsset(spriteAsset, filePath + fileNameWithoutExtension + ".asset");
        }
    }
   
    public static List<SpriteInforGroup> GetAssetSpriteInfor(Texture2D tex, bool isStatic)
    {
        string filePath = UnityEditor.AssetDatabase.GetAssetPath(tex);

        Object[] objects = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(filePath);

        List<SpriteInfor> _tempSprite = new List<SpriteInfor>();

        Vector2 _texSize = new Vector2(tex.width, tex.height);
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i].GetType() != typeof(Sprite))
                continue;
            SpriteInfor temp = new SpriteInfor();
            Sprite sprite = objects[i] as Sprite;
            temp.Id = i;
            temp.Name = sprite.name;
            temp.Pivot = sprite.pivot;
            temp.Rect = sprite.rect;
            temp.Sprite = sprite;
            temp.Tag = sprite.name;
            temp.Uv = GetSpriteUV(_texSize, sprite.rect);
            _tempSprite.Add(temp);
        }

        List<SpriteInforGroup> _listGroup = null;
        try
        {
            if (isStatic)
                _listGroup = GetStaticList(_tempSprite);
            else
                _listGroup = GetAnimationList(_tempSprite);
        }
        catch(System.Exception e)
        {
            Debug.LogError(e);
        }

        return _listGroup;
    }

    static List<SpriteInforGroup> GetStaticList(List<SpriteInfor> _tempSprite)
    {
        List<SpriteInforGroup> _listGroup = new List<SpriteInforGroup>(_tempSprite.Count);
        for (int i = 0; i < _tempSprite.Count; i++)
        {
            SpriteInforGroup _tempGroup = new SpriteInforGroup();
            _tempGroup.Tag = _tempSprite[i].Tag;
            //_tempGroup.Size = 24.0f;
            //_tempGroup.Width = 1.0f;
            _tempGroup.ListSpriteInfor = new List<SpriteInfor>();
            _tempGroup.ListSpriteInfor.Add(_tempSprite[i]);
            _listGroup.Add(_tempGroup);
        }

        return _listGroup;
    }

    static List<SpriteInforGroup> GetAnimationList(List<SpriteInfor> spriteList)
    {
        SortedDictionary<string, SpriteInforGroup> dic = new SortedDictionary<string, SpriteInforGroup>();
        for (int i = 0; i < spriteList.Count; i++)
        {
            string[] info = spriteList[i].Name.Split('_');
            if (info.Length != 2)
            {
                Debug.Log("sprite name format error, should be xxx_N, like happy_0, happy_1, etc");
                continue;
            }

            string tag = info[0];
            spriteList[i].Tag = tag;
            if(dic.ContainsKey(tag))
            {
                dic[tag].ListSpriteInfor.Insert(int.Parse(info[1]), spriteList[i]);
            }
            else
            {
                SpriteInforGroup group = new SpriteInforGroup();
                group.Tag = tag;
                group.ListSpriteInfor = new List<SpriteInfor>();
                group.ListSpriteInfor.Insert(int.Parse(info[1]), spriteList[i]);
                dic.Add(tag, group);
            }
        }

        List<SpriteInforGroup> listGroup = new List<SpriteInforGroup>(dic.Count);
        foreach(var v in dic)
        {
            listGroup.Add(v.Value);
        }

        return listGroup;
    }

    private static Vector2[] GetSpriteUV(Vector2 texSize,Rect _sprRect)
    {
        Vector2[] uv = new Vector2[4];
        uv[0] = new Vector2(_sprRect.x / texSize.x, (_sprRect.y+_sprRect.height) / texSize.y);
        uv[1] = new Vector2((_sprRect.x + _sprRect.width) / texSize.x, (_sprRect.y +_sprRect.height) / texSize.y);
        uv[2] = new Vector2((_sprRect.x + _sprRect.width) / texSize.x, _sprRect.y / texSize.y);
        uv[3] = new Vector2(_sprRect.x / texSize.x, _sprRect.y / texSize.y);
        return uv;
    }
    
}
