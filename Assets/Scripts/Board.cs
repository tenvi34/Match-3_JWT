using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class Board : MonoBehaviour
{
    public static Board Instance { get; private set; }

    [SerializeField] private AudioClip popAudio;
    [SerializeField] private AudioSource audioSource;

    public Row[] rows;
    public Tile[,] Tiles { get; private set; }

    public int Width => Tiles.GetLength(0); // 보드의 너비
    public int Height => Tiles.GetLength(1); // 보드의 높이

    private readonly List<Tile> _selection = new List<Tile>(); // 선택된 타일 리스트

    private const float TweenDuration = 0.25f; // 트윈 애니메이션 지속 시간

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Tiles = new Tile[rows.Max(row => row.tiles.Length), rows.Length];

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                var tile = rows[y].tiles[x];

                tile.x = x;
                tile.y = y;

                tile.Item = ItemDatabase.Items[Random.Range(0, ItemDatabase.Items.Length)]; // 랜덤 블럭 설정

                Tiles[x, y] = tile; // 타일 배열에 블럭 추가
            }
        }
        
        // 시작할 때 생성된 블럭 중 일치하는것은 처리 후 시작
        Pop();
    }

    public async void Select(Tile tile)
    {
        // 근처에 있는 타일만 움직이게 설정
        if (!_selection.Contains(tile))
        {
            if (_selection.Count > 0)
            {
                if (Array.IndexOf(_selection[0].LRTB, tile) != -1)
                {
                    _selection.Add(tile);
                }
            }
            else
            {
                _selection.Add(tile);
            }
        }
        
        if (_selection.Count < 2) return; // 선택된 타일이 두개가 아니면 종료

        Debug.Log($"선택한 타일: ({_selection[0].x}, {_selection[0].y}) : ({_selection[1].x}, {_selection[1].y})");

        await Swap(_selection[0], _selection[1]); // Swap

        if (IsPop()) Pop(); // 매칭이 되면 Pop 실행
        else await Swap(_selection[0], _selection[1]); // 매칭이 안되면 원래대로 돌아가기

        _selection.Clear();
    }

    public async Task Swap(Tile tile1, Tile tile2)
    {
        var icon1 = tile1.icon;
        var icon2 = tile2.icon;

        var icon1Transform = icon1.transform;
        var icon2Transform = icon2.transform;

        var sequence = DOTween.Sequence();

        // Swap 애니메이션
        sequence.Join(icon1Transform.DOMove(icon2Transform.position, TweenDuration))
            .Join(icon2Transform.DOMove(icon1Transform.position, TweenDuration));

        await sequence.Play().AsyncWaitForCompletion();

        icon1Transform.SetParent(tile2.transform);
        icon2Transform.SetParent(tile1.transform);

        tile1.icon = icon2;
        tile2.icon = icon1;

        // 블럭 Swap
        (tile1.Item, tile2.Item) = (tile2.Item, tile1.Item);
    }

    private bool IsPop()
    {
        // 매칭 확인
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                if (Tiles[x, y].GetConnectedTiles().Skip(1).Count() >= 2) return true; // 3개 이상 연결되었는지 확인
            }
        }

        return false; 
    }

    private async void Pop()
    {
        // 매칭된 타일 제거
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                var tile = Tiles[x, y];
                var connectedTiles = tile.GetConnectedTiles();
                
                if (connectedTiles.Skip(1).Count() < 2) continue; // 3개 이상이 아니면 무시

                var deflateSequence = DOTween.Sequence();
                foreach (var connectedTile in connectedTiles)
                {
                    deflateSequence.Join(connectedTile.icon.transform.DOScale(Vector3.zero, TweenDuration)); // 매치된 블럭 제거
                }

                await deflateSequence.Play().AsyncWaitForCompletion();

                audioSource.PlayOneShot(popAudio); 
                ScoreScript.Instance.Score += tile.Item.value * connectedTiles.Count; 
                
                var inflateSequence = DOTween.Sequence();
                foreach (var connectedTile in connectedTiles)
                {
                    connectedTile.Item = ItemDatabase.Items[Random.Range(0, ItemDatabase.Items.Length)];
                    inflateSequence.Join(connectedTile.icon.transform.DOScale(Vector3.one, TweenDuration)); // 새로운 블럭 채우기
                }

                await inflateSequence.Play().AsyncWaitForCompletion();
            }
        }
    }
}
