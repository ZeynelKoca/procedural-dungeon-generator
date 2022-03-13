using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Assets.Scripts.Tiles;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Assets.Scripts
{
    public class DungeonGenerator : MonoBehaviour
    {
        private const int MAX_LOOP_RUNS = 1000;

        [SerializeField]
        private CanvasUpdater canvasUpdater;
        [SerializeField] 
        public Sprite sprite;
        [SerializeField]
        public Text tilesOffsetText;

        private int _dungeonSize;
        private ITileType[,] _dungeonArray;
        private GameObject[,] _dungeonTileGameObjects;
        private GameObject _gridHolder;
        private float _tileSize;
        private int _horizontal;
        private int _vertical;
        private float _tilesOffset;

        // Start is called before the first frame update
        void Start()
        {
            _gridHolder = GameObject.Find("GridHolder");
            _tilesOffset = 0;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }

        public void UpdateTilesOffset(float value)
        {
            _tilesOffset = value;
            tilesOffsetText.text = value.ToString(CultureInfo.CurrentCulture);
        }

        public void GenerateDungeon()
        {
            _dungeonSize = canvasUpdater.DungeonSize;
            ClearPreviousDungeon();
            CreateEmptyGrid();
            CreateDungeon();
        }

        private void ClearPreviousDungeon()
        {
            if (_dungeonTileGameObjects != null)
            {
                foreach (var dungeonTileGameObject in _dungeonTileGameObjects)
                {
                    if (dungeonTileGameObject != null)
                    {
                        dungeonTileGameObject.transform.parent = null;
                        Destroy(dungeonTileGameObject);
                    }
                }
            }
        }

        private void CreateEmptyGrid()
        {
            // Calculate individual tile sizes according to the dungeon size
            CalculateTileSize();
            _dungeonArray = new ITileType[_dungeonSize, _dungeonSize];
            _dungeonTileGameObjects = new GameObject[_dungeonSize, _dungeonSize];
            for (int x = 0; x < _dungeonSize; x++)
            {
                for (int y = 0; y < _dungeonSize; y++)
                {
                    // Fill grid with empty tile objects.
                    _dungeonArray[x, y] = new EmptyTile();
                    SpawnTile(x, y, _dungeonArray[x, y]);
                }
            }
        }

        private void CalculateTileSize()
        {
            _vertical = (int)Camera.main!.orthographicSize;
            _horizontal = _vertical * (Screen.width / Screen.height);
            if (_dungeonSize > 9)
            {
                _tileSize = (float)_vertical / _dungeonSize * 1.7f;
            }
            else
            {
                _tileSize = 1f;
            }
        }

        private void CreateDungeon()
        {
            // Room size +2 for the walls on each side.
            var firstRoomLength = Random.Range(canvasUpdater.MinRoomLength + 2, canvasUpdater.MaxRoomLength + 2);
            var firstRoomHeight = Random.Range(canvasUpdater.MinRoomHeight + 2, canvasUpdater.MaxRoomHeight + 2);
            var mapCenter = _dungeonSize / 2;
            CreateRoom(Direction.Up, mapCenter, mapCenter, firstRoomLength, firstRoomHeight);

            // i=1 for the first room that was just created.
            for (int i = 1; i < canvasUpdater.RoomsAmount; i++)
            {
                for (int loopRun = 0; loopRun < MAX_LOOP_RUNS; loopRun++)
                {
                    int newRoomBranchX = Random.Range(0, _dungeonSize - 1);
                    int newRoomBranchY = Random.Range(0, _dungeonSize - 1);

                    // Only branch a new room from the wall of a different room or from an existing hallway.
                    if (GetTileType(newRoomBranchX, newRoomBranchY) is WallTile || GetTileType(newRoomBranchX, newRoomBranchY) is HallwayTile)
                    {
                        int newRoomLength = Random.Range(canvasUpdater.MinRoomLength + 2, canvasUpdater.MaxRoomLength + 2);
                        int newRoomHeight = Random.Range(canvasUpdater.MinRoomHeight + 2, canvasUpdater.MaxRoomHeight + 2);
                        var surroundings = GetSurroundings(new Vector2(newRoomBranchX, newRoomBranchY));

                        var roomPlaced = surroundings.FirstOrDefault(s => s.Item3 is EmptyTile
                                                                         && TryPlaceRoom(s.Item2, (int)s.Item1.x, (int)s.Item1.y,
                                                                             newRoomLength, newRoomHeight));
                        if (roomPlaced == null)
                        {
                            // Couldn't create a room by using this loop's RNG, try again in the next loop.
                            continue;
                        }

                        // Room and corresponding hallway were created successfully.
                        break;
                    }
                }
            }
        }

        private bool TryPlaceRoom(Direction direction, int xPos, int yPos, int roomLength, int roomHeight)
        {
            var hallwayLength = Random.Range(canvasUpdater.MinHallwayLength, canvasUpdater.MaxHallwayLength);
            switch (direction)
            {
                case Direction.Up:
                    if (CreateRoom(direction, xPos, yPos + hallwayLength, roomLength, roomHeight))
                    {
                        if (!CreateHallway(direction, xPos, yPos, hallwayLength))
                        {
                            // Couldn't create a hallway leading to the current room, so remove the current room
                            RemoveRoom(direction, xPos, yPos + hallwayLength, roomLength, roomHeight);
                            return false;
                        }
                        return true;
                    }

                    return false;
                case Direction.Down:
                    if (CreateRoom(direction, xPos, yPos - hallwayLength, roomLength, roomHeight))
                    {
                        if (!CreateHallway(direction, xPos, yPos, hallwayLength))
                        {
                            // Couldn't create a hallway leading to the current room, so remove the current room
                            RemoveRoom(direction, xPos, yPos - hallwayLength, roomLength, roomHeight);
                            return false;
                        }
                        return true;
                    }

                    return false;
                case Direction.Right:
                    if (CreateRoom(direction, xPos + hallwayLength, yPos, roomLength, roomHeight))
                    {
                        if (!CreateHallway(direction, xPos, yPos, hallwayLength))
                        {
                            // Couldn't create a hallway leading to the current room, so remove the current room
                            RemoveRoom(direction, xPos + hallwayLength, yPos, roomLength, roomHeight);
                            return false;
                        }
                        return true;
                    }

                    return false;
                case Direction.Left:
                    if (CreateRoom(direction, xPos - hallwayLength, yPos, roomLength, roomHeight))
                    {
                        if (!CreateHallway(direction, xPos, yPos, hallwayLength))
                        {
                            // Couldn't create a hallway leading to the current room, so remove the current room
                            RemoveRoom(direction, xPos - hallwayLength, yPos, roomLength, roomHeight);
                            return false;
                        }
                        return true;
                    }

                    return false;
                default:
                    throw new InvalidOperationException();
            }
        }

        private bool CreateRoom(Direction direction, int xPos, int yPos, int roomLength, int roomHeight)
        {
            // Gets all grid coordinates that will make up the current room.
            var roomCoordinatePoints = GetRoomPoints(direction, xPos, yPos, roomLength, roomHeight).ToArray();

            if (roomCoordinatePoints.Any(v =>
                v.y < 0 || v.x < 0 || v.x > _dungeonSize || v.y > _dungeonSize ||
                !(GetTileType(v.x, v.y) is EmptyTile)))
            {
                // Don't create a room if it's not within the dungeon size's bounds
                // or if the area is already filled with tiles (i.e. room tiles, hallway tiles).
                return false;
            }

            foreach (var coordinate in roomCoordinatePoints)
            {
                var xCoordinate = (int)coordinate.x;
                var yCoordinate = (int)coordinate.y;
                ITileType tile;
                if (IsWall(direction, xPos, yPos, xCoordinate, yCoordinate, roomLength, roomHeight))
                {
                    tile = new WallTile();
                }
                else
                {
                    tile = new RoomTile();
                }
                _dungeonArray[xCoordinate, yCoordinate] = tile;
                SpawnTile(xCoordinate, yCoordinate, _dungeonArray[xCoordinate, yCoordinate]);
            }

            // Room created successfully
            return true;
        }

        private IEnumerable<Vector2> GetRoomPoints(Direction direction, int xPos, int yPos, int roomLength, int roomHeight)
        {
            Func<int, int, int> lowerBound = GetLowerBoundRoom;
            Func<int, int, int> upperBound = GetUpperBoundRoom;

            switch (direction)
            {
                case Direction.Up:
                    for (int x = lowerBound(xPos, roomLength); x < upperBound(xPos, roomLength); x++)
                    {
                        for (int y = yPos; y < yPos + roomHeight; y++)
                        {
                            yield return new Vector2(x, y);
                        }
                    }

                    break;
                case Direction.Down:
                    for (int x = lowerBound(xPos, roomLength); x < upperBound(xPos, roomLength); x++)
                    {
                        for (int y = yPos; y > yPos - roomHeight; y--)
                        {
                            yield return new Vector2(x, y);
                        }
                    }

                    break;
                case Direction.Right:
                    for (int x = xPos; x < xPos + roomLength; x++)
                    {
                        for (int y = lowerBound(yPos, roomHeight); y < upperBound(yPos, roomHeight); y++)
                        {
                            yield return new Vector2(x, y);
                        }
                    }

                    break;
                case Direction.Left:
                    for (int x = xPos; x > xPos - roomLength; x--)
                    {
                        for (int y = lowerBound(yPos, roomHeight); y < upperBound(yPos, roomHeight); y++)
                        {
                            yield return new Vector2(x, y);
                        }
                    }

                    break;
            }
        }

        private void RemoveRoom(Direction direction, int xPos, int yPos, int roomLength, int roomHeight)
        {
            var roomCoordinatePoints = GetRoomPoints(direction, xPos, yPos, roomLength, roomHeight).ToArray();

            foreach (var coordinate in roomCoordinatePoints)
            {
                var xCoordinate = (int)coordinate.x;
                var yCoordinate = (int)coordinate.y;
                _dungeonArray[xCoordinate, yCoordinate] = new EmptyTile();
                SpawnTile(xCoordinate, yCoordinate, _dungeonArray[xCoordinate, yCoordinate]);
            }
        }

        private bool CreateHallway(Direction direction, int xPos, int yPos, int hallwayLength)
        {
            if (_dungeonArray[xPos, yPos] is RoomTile)
            {
                return false;
            }
            if (IsHallwayConnectedToRoomCorner(direction, xPos, yPos, hallwayLength))
            {
                // Hallway trying to connect to the corner wall of a room, so don't create it.
                return false;
            }

            switch (direction)
            {
                case Direction.Up:
                    for (int y = yPos - 1; y <= yPos + hallwayLength; y++)
                    {
                        if (!CreateHallwayTile(xPos, y))
                        {
                            return false;
                        }
                    }

                    return true;
                case Direction.Down:
                    for (int y = yPos + 1; y >= yPos - hallwayLength; y--)
                    {
                        if (!CreateHallwayTile(xPos, y))
                        {
                            return false;
                        }
                    }

                    return true;
                case Direction.Right:
                    for (int x = xPos - 1; x <= xPos + hallwayLength; x++)
                    {
                        if (!CreateHallwayTile(x, yPos))
                        {
                            return false;
                        }
                    }

                    return true;
                case Direction.Left:
                    for (int x = xPos + 1; x >= xPos - hallwayLength; x--)
                    {
                        if (!CreateHallwayTile(x, yPos))
                        {
                            return false;
                        }
                    }

                    return true;
                default:
                    throw new InvalidOperationException();
            }
        }

        private bool CreateHallwayTile(int x, int y)
        {
            if (_dungeonArray[x, y] is RoomTile)
            {
                return false;
            }

            _dungeonArray[x, y] = new HallwayTile();
            SpawnTile(x, y, _dungeonArray[x, y]);
            SurroundWithWalls(x, y);
            return true;
        }

        private bool IsHallwayConnectedToRoomCorner(Direction direction, int x, int y, int hallwayLength)
        {
            switch (direction)
            {
                case Direction.Up:
                    return _dungeonArray[x, y - 2] is WallTile ||
                           _dungeonArray[x, y + hallwayLength + 2] is WallTile;
                case Direction.Down:
                    return _dungeonArray[x, y + 2] is WallTile ||
                           _dungeonArray[x, y - hallwayLength - 2] is WallTile;
                case Direction.Right:
                    return _dungeonArray[x - 2, y] is WallTile ||
                           _dungeonArray[x + hallwayLength + 2, y] is WallTile;
                case Direction.Left:
                    return _dungeonArray[x + 2, y] is WallTile ||
                           _dungeonArray[x - hallwayLength - 2, y] is WallTile;
                default:
                    throw new InvalidOperationException();
            }
        }

        private IEnumerable<Tuple<Vector2, Direction>> GetSurroundingCoordinates(Vector2 coordinate)
        {
            var coordinates = new[]
            {
                Tuple.Create(new Vector2(coordinate.x, coordinate.y + 1), Direction.Up),
                Tuple.Create(new Vector2(coordinate.x - 1, coordinate.y), Direction.Left),
                Tuple.Create(new Vector2(coordinate.x, coordinate.y - 1), Direction.Down),
                Tuple.Create(new Vector2(coordinate.x + 1, coordinate.y), Direction.Right)
            };
            return coordinates.Where(p => InDungeonBounds(p.Item1));
        }

        private IEnumerable<Tuple<Vector2, Direction, ITileType>> GetSurroundings(Vector2 coordinate)
        {
            return GetSurroundingCoordinates(coordinate)
                    .Select(r => Tuple.Create(r.Item1, r.Item2, GetTileType(r.Item1.x, r.Item1.y)));
        }

        private bool InDungeonBounds(int x, int y)
        {
            return x > 0 && x < _dungeonSize && y > 0 && y < _dungeonSize;
        }

        private bool InDungeonBounds(Vector2 coordinates)
        {
            return InDungeonBounds((int)coordinates.x, (int)coordinates.y);
        }

        private ITileType GetTileType(float xCoordinate, float yCoordinate)
        {
            return GetTileType((int)xCoordinate, (int)yCoordinate);
        }
        
        private ITileType GetTileType(int xCoordinate, int yCoordinate)
        {
            try
            {
                return _dungeonArray[xCoordinate, yCoordinate];
            }
            catch
            {
                return null;
            }
        }

        private void SurroundWithWalls(int x, int y)
        {
            var surroundings = GetSurroundings(new Vector2(x, y));
            foreach (var s in surroundings)
            {
                if (s.Item3 is EmptyTile)
                {
                    // Only surround with walls if the surrounding tiles consist of Empty Tiles
                    // (otherwise room/hallway tiles would get overwritten)
                    _dungeonArray[(int)s.Item1.x, (int)s.Item1.y] = new WallTile();
                    SpawnTile((int)s.Item1.x, (int)s.Item1.y, _dungeonArray[(int)s.Item1.x, (int)s.Item1.y]);
                }
            }
        }

        private bool IsWall(Direction direction, int xOrigin, int yOrigin, int x, int y, int roomLength, int roomHeight)
        {
            Func<int, int, int> lowerBound = GetLowerBoundRoom;
            Func<int, int, int> isTileWallBound = IsTileWallBound;

            switch (direction)
            {
                case Direction.Up:
                    return x == lowerBound(xOrigin, roomLength) || x == isTileWallBound(xOrigin, roomLength) ||
                           y == yOrigin || y == yOrigin + roomHeight - 1;
                case Direction.Down:
                    return x == lowerBound(xOrigin, roomLength) || x == isTileWallBound(xOrigin, roomLength) ||
                           y == yOrigin || y == yOrigin - roomHeight + 1;
                case Direction.Right:
                    return x == xOrigin || x == xOrigin + roomLength - 1 || y == lowerBound(yOrigin, roomHeight) ||
                           y == isTileWallBound(yOrigin, roomHeight);
                case Direction.Left:
                    return x == xOrigin || x == xOrigin - roomLength + 1 || y == lowerBound(yOrigin, roomHeight) ||
                           y == isTileWallBound(yOrigin, roomHeight);
                default:
                    throw new InvalidOperationException();
            }
        }

        private int IsTileWallBound(int coordinate, int length)
        {
            return coordinate + (length - 1) / 2;
        }

        private int GetUpperBoundRoom(int coordinate, int length)
        {
            return coordinate + (length + 1) / 2;
        }

        private int GetLowerBoundRoom(int coordinate, int length)
        {
            return coordinate - length / 2;
        }

        private void SpawnTile(int x, int y, ITileType tileType)
        {
            if (_dungeonTileGameObjects[x, y] != null)
            {
                // Tile is already filled with something. Destroy the gameObject for performance reasons.
                _dungeonTileGameObjects[x, y].transform.parent = null;
                Destroy(_dungeonTileGameObjects[x, y]);
            }
            var tilesOffset = _tileSize * (1 + _tilesOffset / 100);
            var g = new GameObject($"X: {x} Y: {y} - {tileType.GetType().Name}");
            g.transform.parent = _gridHolder.transform;
            g.transform.position = new Vector3(x * tilesOffset - (_horizontal - 0.5f),
                y * tilesOffset - (_vertical - 0.5f));
            g.transform.localScale = new Vector3(_tileSize, _tileSize, _tileSize);
            _dungeonTileGameObjects[x, y] = g;
            var s = g.AddComponent<SpriteRenderer>();
            s.sprite = sprite;
            s.color = tileType.Color;
        }
    }
}