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
        if (chunks.Count == 0)
            return [];

        var totalLength = chunks.Sum(c => c.Length - 1);
        if (totalLength < 0)
            throw new InvalidOperationException("Invalid chunk lengths.");

        var result = new byte[totalLength];
        var offset = 0;
        var lastIndex = chunks.Count - 1;

        for (var index = 0; index < chunks.Count; index++)
        {
            var chunk = chunks[index];
            if (chunk.Length < 1)
                throw new InvalidOperationException($"Chunk at index {index} is too small to contain a flag byte.");

            var flag = chunk[0];
            var isLast = index == lastIndex;

            if (isLast && flag != LastChunkFlag)
                throw new InvalidOperationException("Last chunk does not have the correct LastChunkFlag.");
            if (!isLast && flag != MoreIncomingFlag)
                throw new InvalidOperationException($"Chunk {index} does not have the expected MoreIncomingFlag.");

            Buffer.BlockCopy(chunk, 1, result, offset, chunk.Length - 1);
            offset += chunk.Length - 1;
        }

        return result;
    }


    public static bool MoreIncomingFlagIsChecked(byte[] bytes)
    {
        return bytes is { Length: > 0 } && bytes[0] == MoreIncomingFlag;
    }

    public static bool LastChunkFlagIsChecked(byte[] bytes)
    {
        return bytes is { Length: > 0 } && bytes[0] == LastChunkFlag;
    }
}
