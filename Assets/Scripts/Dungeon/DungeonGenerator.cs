using System;
using System.Collections.Generic;
using System.Linq;

public sealed class DungeonGenerator
{
    public DungeonLayout Generate(int seed, int minimumVisibleRooms = 8, int maximumVisibleRooms = 12)
    {
        if (minimumVisibleRooms < 6 || maximumVisibleRooms < minimumVisibleRooms)
        {
            throw new ArgumentOutOfRangeException(nameof(minimumVisibleRooms));
        }

        Random random = new Random(seed);
        int targetRoomCount = random.Next(minimumVisibleRooms, maximumVisibleRooms + 1);
        DungeonLayout layout = new DungeonLayout(seed);
        RoomCoordinate start = new RoomCoordinate(0, 0);
        layout.StartCoordinate = start;
        layout.AddRoom(new RoomNode(start, RoomType.Start));

        // Reserve the diagonal of two perpendicular start-room neighbors. The
        // reserved cell can never be consumed by a visible room, so the secret
        // room always has at least two orthogonally adjacent rooms.
        RoomDirection firstDirection = RoomDirectionUtility.All[random.Next(RoomDirectionUtility.All.Count)];
        RoomDirection secondDirection = GetRandomPerpendicularDirection(firstDirection, random);
        RoomCoordinate firstNeighbor = start.Offset(firstDirection);
        RoomCoordinate secondNeighbor = start.Offset(secondDirection);
        RoomCoordinate firstOffset = RoomDirectionUtility.ToOffset(firstDirection);
        RoomCoordinate secondOffset = RoomDirectionUtility.ToOffset(secondDirection);
        RoomCoordinate reservedSecretCoordinate = new RoomCoordinate(
            start.X + firstOffset.X + secondOffset.X,
            start.Y + firstOffset.Y + secondOffset.Y);

        layout.AddRoom(new RoomNode(firstNeighbor, RoomType.Combat));
        layout.AddRoom(new RoomNode(secondNeighbor, RoomType.Combat));
        List<RoomCoordinate> visibleCoordinates = new List<RoomCoordinate>
        {
            start,
            firstNeighbor,
            secondNeighbor
        };
        // Build the main graph first and reserve the final visible-room slot for
        // a terminal boss chamber.
        while (visibleCoordinates.Count < targetRoomCount - 1)
        {
            RoomCoordinate source = visibleCoordinates[random.Next(visibleCoordinates.Count)];
            RoomDirection direction = RoomDirectionUtility.All[random.Next(RoomDirectionUtility.All.Count)];
            RoomCoordinate candidate = source.Offset(direction);
            if (candidate == reservedSecretCoordinate || layout.TryGetRoom(candidate, out _)) continue;

            layout.AddRoom(new RoomNode(candidate, RoomType.Combat));
            visibleCoordinates.Add(candidate);
        }

        // Door topology must exist before the Boss position is selected so
        // "farthest" is measured by shortest-path room depth, not coordinates.
        ConnectAllAdjacentVisibleRooms(layout);
        RoomNode bossEntranceRoom = AddBossRoomData(
            layout,
            visibleCoordinates,
            reservedSecretCoordinate,
            random);
        // Connect the newly appended terminal chamber. Its candidate position
        // was required to have exactly one visible neighbor.
        ConnectAllAdjacentVisibleRooms(layout);
        AddSecretRoomData(layout, visibleCoordinates, reservedSecretCoordinate, random);
        RoomNode superSecretEntranceRoom = AddSuperSecretRoomData(layout, random);
        AssignSpecialVisibleRooms(layout, start, bossEntranceRoom, superSecretEntranceRoom, random);
        return layout;
    }

    private static RoomDirection GetRandomPerpendicularDirection(RoomDirection direction, Random random)
    {
        bool horizontal = direction == RoomDirection.West || direction == RoomDirection.East;
        if (horizontal) return random.Next(0, 2) == 0 ? RoomDirection.North : RoomDirection.South;
        return random.Next(0, 2) == 0 ? RoomDirection.West : RoomDirection.East;
    }

    private static void ConnectAllAdjacentVisibleRooms(DungeonLayout layout)
    {
        // Layout is finalized before doors are decided. Scanning only north and
        // east visits every adjacent pair once; DungeonLayout.Connect adds both
        // directions so the corresponding doors always agree.
        RoomDirection[] forwardDirections = { RoomDirection.North, RoomDirection.East };
        foreach (RoomNode room in layout.VisibleRooms.ToList())
        {
            foreach (RoomDirection direction in forwardDirections)
            {
                if (layout.TryGetRoom(room.Coordinate.Offset(direction), out RoomNode neighbor) &&
                    !RoomTypeUtility.IsHiddenRoom(neighbor.Type))
                {
                    layout.Connect(room.Coordinate, direction);
                }
            }
        }
    }

    private static RoomNode AddBossRoomData(
        DungeonLayout layout,
        List<RoomCoordinate> visibleCoordinates,
        RoomCoordinate reservedSecretCoordinate,
        Random random)
    {
        List<BossRoomCandidate> candidates = layout.VisibleRooms
            .Where(room => room.Coordinate != layout.StartCoordinate)
            .SelectMany(room => RoomDirectionUtility.All.Select(direction => room.Coordinate.Offset(direction)))
            .Distinct()
            .Where(coordinate => coordinate != reservedSecretCoordinate &&
                                 coordinate.ManhattanDistance(reservedSecretCoordinate) > 1 &&
                                 !layout.TryGetRoom(coordinate, out _))
            .Select(coordinate =>
            {
                List<RoomNode> neighbors = RoomDirectionUtility.All
                    .Select(direction => layout.TryGetRoom(coordinate.Offset(direction), out RoomNode neighbor) ? neighbor : null)
                    .Where(neighbor => neighbor != null && !RoomTypeUtility.IsHiddenRoom(neighbor.Type))
                    .ToList();
                return new BossRoomCandidate(coordinate, neighbors);
            })
            .Where(candidate => candidate.Neighbors.Count == 1 &&
                                candidate.Neighbors[0].Coordinate != layout.StartCoordinate)
            .ToList();

        if (candidates.Count == 0)
        {
            throw new InvalidOperationException("The generated graph has no valid terminal boss-room position.");
        }

        Dictionary<RoomCoordinate, int> graphDistances = CalculateVisibleGraphDistances(layout);
        int bestDistance = candidates.Max(candidate => graphDistances[candidate.Neighbors[0].Coordinate] + 1);
        List<BossRoomCandidate> preferred = candidates
            .Where(candidate => graphDistances[candidate.Neighbors[0].Coordinate] + 1 == bestDistance)
            .ToList();
        int bestCoordinateDistance = preferred.Max(candidate =>
            candidate.Coordinate.ManhattanDistance(layout.StartCoordinate));
        preferred = preferred
            .Where(candidate => candidate.Coordinate.ManhattanDistance(layout.StartCoordinate) == bestCoordinateDistance)
            .ToList();
        BossRoomCandidate selected = preferred[random.Next(preferred.Count)];
        layout.AddRoom(new RoomNode(selected.Coordinate, RoomType.Boss));
        visibleCoordinates.Add(selected.Coordinate);
        return selected.Neighbors[0];
    }

    private static Dictionary<RoomCoordinate, int> CalculateVisibleGraphDistances(DungeonLayout layout)
    {
        Dictionary<RoomCoordinate, int> distances = new Dictionary<RoomCoordinate, int>
        {
            [layout.StartCoordinate] = 0
        };
        Queue<RoomCoordinate> pending = new Queue<RoomCoordinate>();
        pending.Enqueue(layout.StartCoordinate);
        while (pending.Count > 0)
        {
            RoomNode current = layout.Rooms[pending.Dequeue()];
            foreach (RoomDirection direction in current.Connections)
            {
                if (layout.TryGetConnectedRoom(current, direction, out RoomNode neighbor) &&
                    !RoomTypeUtility.IsHiddenRoom(neighbor.Type) &&
                    !distances.ContainsKey(neighbor.Coordinate))
                {
                    distances[neighbor.Coordinate] = distances[current.Coordinate] + 1;
                    pending.Enqueue(neighbor.Coordinate);
                }
            }
        }

        if (distances.Count != layout.VisibleRooms.Count())
        {
            throw new InvalidOperationException("Visible room graph is disconnected before Boss placement.");
        }
        return distances;
    }

    private static void AssignSpecialVisibleRooms(
        DungeonLayout layout,
        RoomCoordinate start,
        RoomNode bossEntranceRoom,
        RoomNode superSecretEntranceRoom,
        Random random)
    {
        List<RoomNode> candidates = layout.VisibleRooms
            .Where(room => room.Coordinate != start &&
                           room.Type != RoomType.Boss &&
                           room != bossEntranceRoom &&
                           room != superSecretEntranceRoom)
            .ToList();

        // Orthogonal grid neighbors always have opposite coordinate parity.
        // Selecting Item and Shop from one parity group makes them non-adjacent.
        // The Boss has exactly one adjacent room and that room is excluded here,
        // so neither visible special room can touch the Boss.
        List<SpecialRoomSet> validSets = candidates
            .GroupBy(room => (room.Coordinate.X + room.Coordinate.Y) & 1)
            .Where(group => group.Count() >= 2)
            .Select(group =>
            {
                RoomNode[] rooms = group
                    .OrderByDescending(room => room.Coordinate.ManhattanDistance(start))
                    .ThenBy(room => room.Coordinate.X)
                    .ThenBy(room => room.Coordinate.Y)
                    .Take(2)
                    .ToArray();
                return new SpecialRoomSet(
                    rooms,
                    rooms.Sum(room => room.Coordinate.ManhattanDistance(start)));
            })
            .ToList();

        if (validSets.Count == 0)
        {
            throw new InvalidOperationException("The generated graph has no two-room independent set for Item and Shop.");
        }

        int bestScore = validSets.Max(set => set.DistanceScore);
        List<SpecialRoomSet> preferredSets = validSets.Where(set => set.DistanceScore == bestScore).ToList();
        RoomNode[] selected = preferredSets[random.Next(preferredSets.Count)].Rooms;
        bool swapTypes = random.Next(0, 2) == 1;
        (swapTypes ? selected[1] : selected[0]).SetType(RoomType.Item);
        (swapTypes ? selected[0] : selected[1]).SetType(RoomType.Shop);
    }

    private static void AddSecretRoomData(
        DungeonLayout layout,
        List<RoomCoordinate> visibleCoordinates,
        RoomCoordinate reservedSecretCoordinate,
        Random random)
    {
        int minimumX = visibleCoordinates.Min(coordinate => coordinate.X) - 1;
        int maximumX = visibleCoordinates.Max(coordinate => coordinate.X) + 1;
        int minimumY = visibleCoordinates.Min(coordinate => coordinate.Y) - 1;
        int maximumY = visibleCoordinates.Max(coordinate => coordinate.Y) + 1;

        List<SecretCandidate> candidates = new List<SecretCandidate>();
        for (int x = minimumX; x <= maximumX; x++)
        {
            for (int y = minimumY; y <= maximumY; y++)
            {
                RoomCoordinate coordinate = new RoomCoordinate(x, y);
                if (layout.TryGetRoom(coordinate, out _)) continue;

                int adjacentVisibleRooms = RoomDirectionUtility.All.Count(direction =>
                    layout.TryGetRoom(coordinate.Offset(direction), out RoomNode neighbor) &&
                    !RoomTypeUtility.IsHiddenRoom(neighbor.Type));
                bool touchesBoss = RoomDirectionUtility.All.Any(direction =>
                    layout.TryGetRoom(coordinate.Offset(direction), out RoomNode neighbor) &&
                    neighbor.Type == RoomType.Boss);
                if (adjacentVisibleRooms >= 2 && !touchesBoss)
                {
                    candidates.Add(new SecretCandidate(coordinate, adjacentVisibleRooms));
                }
            }
        }

        if (candidates.All(candidate => candidate.Coordinate != reservedSecretCoordinate))
        {
            throw new InvalidOperationException("The reserved secret-room cell lost its two visible neighbors.");
        }

        int bestAdjacency = candidates.Max(candidate => candidate.AdjacentVisibleRooms);
        List<SecretCandidate> preferred = candidates
            .Where(candidate => candidate.AdjacentVisibleRooms == bestAdjacency)
            .ToList();
        RoomCoordinate secretCoordinate = preferred[random.Next(preferred.Count)].Coordinate;
        layout.AddRoom(new RoomNode(secretCoordinate, RoomType.Secret));
        layout.SecretCoordinate = secretCoordinate;
        // The secret node intentionally has no normal graph connection. Stage F
        // will decide which adjacent wall becomes its bomb-opened entrance.
    }

    private static RoomNode AddSuperSecretRoomData(DungeonLayout layout, Random random)
    {
        RoomNode bossRoom = layout.VisibleRooms.Single(room => room.Type == RoomType.Boss);
        List<SuperSecretCandidate> candidates = layout.VisibleRooms
            .Where(room => room.Type == RoomType.Combat)
            .SelectMany(room => RoomDirectionUtility.All.Select(direction => room.Coordinate.Offset(direction)))
            .Distinct()
            .Where(coordinate => !layout.TryGetRoom(coordinate, out _))
            .Select(coordinate =>
            {
                List<RoomNode> neighbors = RoomDirectionUtility.All
                    .Select(direction => layout.TryGetRoom(coordinate.Offset(direction), out RoomNode neighbor) ? neighbor : null)
                    .Where(neighbor => neighbor != null)
                    .ToList();
                return new SuperSecretCandidate(coordinate, neighbors);
            })
            .Where(candidate => candidate.Neighbors.Count == 1 &&
                                candidate.Neighbors[0].Type == RoomType.Combat)
            .ToList();

        if (candidates.Count == 0)
        {
            throw new InvalidOperationException("The generated graph has no terminal Super Secret room position.");
        }

        int closestBossDistance = candidates.Min(candidate =>
            candidate.Coordinate.ManhattanDistance(bossRoom.Coordinate));
        List<SuperSecretCandidate> preferred = candidates
            .Where(candidate => candidate.Coordinate.ManhattanDistance(bossRoom.Coordinate) == closestBossDistance)
            .ToList();
        SuperSecretCandidate selected = preferred[random.Next(preferred.Count)];
        layout.AddRoom(new RoomNode(selected.Coordinate, RoomType.SuperSecret));
        layout.SuperSecretCoordinate = selected.Coordinate;
        return selected.Neighbors[0];
    }

    private struct SecretCandidate
    {
        public RoomCoordinate Coordinate { get; }
        public int AdjacentVisibleRooms { get; }

        public SecretCandidate(RoomCoordinate coordinate, int adjacentVisibleRooms)
        {
            Coordinate = coordinate;
            AdjacentVisibleRooms = adjacentVisibleRooms;
        }
    }

    private struct BossRoomCandidate
    {
        public RoomCoordinate Coordinate { get; }
        public List<RoomNode> Neighbors { get; }

        public BossRoomCandidate(RoomCoordinate coordinate, List<RoomNode> neighbors)
        {
            Coordinate = coordinate;
            Neighbors = neighbors;
        }
    }

    private struct SuperSecretCandidate
    {
        public RoomCoordinate Coordinate { get; }
        public List<RoomNode> Neighbors { get; }

        public SuperSecretCandidate(RoomCoordinate coordinate, List<RoomNode> neighbors)
        {
            Coordinate = coordinate;
            Neighbors = neighbors;
        }
    }

    private struct SpecialRoomSet
    {
        public RoomNode[] Rooms { get; }
        public int DistanceScore { get; }

        public SpecialRoomSet(RoomNode[] rooms, int distanceScore)
        {
            Rooms = rooms;
            DistanceScore = distanceScore;
        }
    }
}
