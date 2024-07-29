using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public int x;
    public int y;
    
    private Item _item;

    public Item Item
    {
        get => _item;
        set
        {
            if (_item == value) return;
            _item = value;
            icon.sprite = _item.sprite;
        }
    }

    public Image icon;
    public Button button;
    
    public Tile Left => x > 0 ? Board.Instance.Tiles[x - 1, y] : null;
    public Tile Right => x < Board.Instance.Width - 1 ? Board.Instance.Tiles[x + 1, y] : null;
    public Tile Top => y > 0 ? Board.Instance.Tiles[x, y - 1] : null;
    public Tile Bottom => y < Board.Instance.Height - 1 ? Board.Instance.Tiles[x, y + 1] : null;

    // 왼쪽, 오른쪽, 위, 아래 타일 배열
    public Tile[] LRTB => new[]
    {
        Left,
        Right,
        Top,
        Bottom,
    };
    
    private void Start()
    {
        button.onClick.AddListener(() => Board.Instance.Select(this));
    }

    // 연결된 타일
    public List<Tile> GetConnectedTiles(List<Tile> exclude = null)
    {
        var result = new List<Tile> { this, };

        if (exclude == null)
        {
            exclude = new List<Tile> { this, };
        }
        else
        {
            exclude.Add(this);
        }

        // 왼쪽, 오른쪽, 위, 아래 타일들에 대해 연결된 타일 찾기
        foreach (var lrtb in LRTB)
        {
            if (lrtb == null || exclude.Contains(lrtb) || lrtb.Item != Item) continue; // 조건에 맞지 않으면 스킵
            
            result.AddRange(lrtb.GetConnectedTiles(exclude));
        }

        return result;
    }
}