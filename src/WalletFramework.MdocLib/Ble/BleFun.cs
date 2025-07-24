using static WalletFramework.MdocLib.Ble.BleFlags;

namespace WalletFramework.MdocLib.Ble;

public static class BleFun
{
    public static List<byte[]> ChunkBytes(byte[] data, int mtu)
    {
        mtu -= 3;

        var chunks = data
            .Select((value, index) => new { value, index })
            .GroupBy(x => x.index / mtu)
            .Select(group => group.Select(x => x.value))
            .Select(bytes => bytes.Prepend(MoreIncomingFlag).ToArray())
            .ToList();

        chunks[^1][0] = LastChunkFlag;

        return chunks;
    }

    public static byte[] UnchunkBytes(List<byte[]> chunks)
    {
        if (chunks == null || chunks.Count == 0)
            return [];

        var data = new List<byte>();
        var index = 0;
        var lastIndex = chunks.Count - 1;

        foreach (var chunk in chunks)
        {
            if (chunk.Length < 1)
                throw new InvalidOperationException($"Chunk at index {index} is too small to contain a flag byte.");

            var flag = chunk[0];
            var isLast = index == lastIndex;

            if (isLast && flag != LastChunkFlag)
                throw new InvalidOperationException("Last chunk does not have the correct LastChunkFlag.");
            if (!isLast && flag != MoreIncomingFlag)
                throw new InvalidOperationException($"Chunk {index} does not have the expected MoreIncomingFlag.");

            data.AddRange(chunk.Skip(1));
            index++;
        }

        return data.ToArray();
    }

    public static bool MoreIncomingFlagIsChecked(byte[] bytes)
    {
        return bytes[0] == MoreIncomingFlag;
    }

    public static bool LastChunkFlagIsChecked(byte[] bytes)
    {
        return bytes[0] == LastChunkFlag;
    }


}
