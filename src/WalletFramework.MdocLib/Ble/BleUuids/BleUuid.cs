using PeterO.Cbor;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;

namespace WalletFramework.MdocLib.Ble.BleUuids;

public readonly struct BleUuid
{
    private string Value { get; }

    private BleUuid(string value)
    {
        Value = value;
    }

    public override string ToString() => Value;

    public static implicit operator string(BleUuid bleUuid) => bleUuid.Value;

    public static Validation<BleUuid> FromString(string bleUuid)
    {
        if (string.IsNullOrWhiteSpace(bleUuid))
            return new StringIsNullOrWhitespaceError<BleUuid>();

        return new BleUuid(bleUuid);
    }
    
    public static Validation<BleUuid> FromCbor(CBORObject cbor, string name)
    {
        try
        {
            return new BleUuid(BleUuidFun.CborMdlUuidToString(cbor));
        }
        catch (Exception e)
        {
            return new CborIsNotAByteStringError(name, e);
        }
    }

    public CBORObject ToCbor()
    {
        var bytes = Guid.Parse(Value).ToByteArray();
        return CBORObject.FromObject(bytes);
    }
}    

public static class BleUuidFun
{
    public static BleUuid CreateServiceUuid()
    {
        var random = Guid.NewGuid();
        return BleUuid.FromString(random.ToString()).UnwrapOrThrow();
    }

    public static string CborMdlUuidToString(CBORObject cborUuid)
    {
        var bytes = cborUuid.GetByteString();
        if (bytes.Length != 16)
            throw new ArgumentException("UUID must be exactly 16 bytes.");

        // Swap RFC → Guid byte order
        Array.Reverse(bytes, 0, 4);
        Array.Reverse(bytes, 4, 2);
        Array.Reverse(bytes, 6, 2);

        return new Guid(bytes).ToString();
    }

}
