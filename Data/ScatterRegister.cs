// -----------------------------------------------------------------------
// <copyright file="ScatterRegister.cs" company="Fredrik Larsson">
// Copyright (c) 2023 Fredrik Larsson. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

/// <summary>
/// Represents a scatter register reader and writer.
/// </summary>
public class ScatterRegister
{
    private class Entry
    {
        public List<Action<byte[]>> Callbacks { get; set; } = new ();
        public byte[]? Data { get; set; }
        public byte Length { get; set; }
    }

    private readonly Dictionary<int, Entry> _register = new ();

    private ScatterRegister AddRegistry(int address, Action<byte[]>? callback, byte length, byte[]? data)
    {
        if (!_register.TryGetValue(address, out var registry))
        {
            _register[address] = registry = new Entry()
            {
                Length = length,
                Data = data,
            };
        }

        if (callback != null)
        {
            registry.Callbacks.Add(callback);
        }

        return this;
    }

    public void Clear()
    {
        _register.Clear();
    }

    /// <summary>
    /// Reads data from the register using the provided callback.
    /// </summary>
    /// <param name="readRegisterCallback">The function to use to read data from the register. This function should take the starting address and length to read as parameters and return the read bytes.</param>
    /// <exception cref="System.ArgumentNullException">Thrown when readRegisterCallback is null.</exception>
    public void Read(Func<int, int, byte[]> readRegisterCallback)
    {
        if (readRegisterCallback == null)
            throw new ArgumentNullException(nameof(readRegisterCallback));

        var addresses = _register
            .OrderBy(x => x.Key)
            .ToArray();

        var chunkLength = 10;
        for (var i = 0; i < addresses.Length; i++)
        {
            var startAddress = addresses[i].Key;

            var j = i + 1;
            while (j < addresses.Length && addresses[j].Key - startAddress <= chunkLength)
            {
                j++;
            }

            var block = readRegisterCallback(startAddress, addresses[j - 1].Key - startAddress + addresses[j - 1].Value.Length);

            for (var k = i; k < j; k++)
            {
                var registry = addresses[k];

                var data = new byte[registry.Value.Length];
                Array.Copy(block, registry.Key - startAddress, data, 0, data.Length);

                foreach (var callback in _register[addresses[k].Key].Callbacks)
                {
                    callback?.Invoke(data);
                }
            }

            i = j - 1;
        }
    }

    /// <summary>
    /// Writes data to the register using the provided callback.
    /// </summary>
    /// <param name="writeRegisterCallback">The action to use to write data to the register. This action should take the starting address and bytes to write as parameters.</param>
    /// <exception cref="System.ArgumentNullException">Thrown when writeRegisterCallback is null.</exception>
    public void Write(Action<int, byte[]> writeRegisterCallback)
    {
        foreach(var (startAddress, registerEntry) in _register)
        {
            var data = registerEntry.Data;
            if (data == null)
            {
                continue;
            }

            writeRegisterCallback(startAddress, data);
        }
    }

    /// <summary>
    /// Reads a byte from the specified address in the register.
    /// </summary>
    /// <param name="address">The address to read the byte from.</param>
    /// <param name="value">The action to perform with the read byte.</param>
    /// <returns>This ScatterRegister object.</returns>
    /// <exception cref="System.ArgumentException">Thrown when the specified address is invalid.</exception>
    public ScatterRegister ReadByte(int address, Action<byte> value) => AddRegistry(
        address, data => value(data[0]), sizeof(byte), null);

    /// <summary>
    /// Reads a ushort from the specified address in the register.
    /// </summary>
    /// <param name="address">The address to read the ushort from.</param>
    /// <param name="value">The action to perform with the read ushort.</param>
    /// <returns>This ScatterRegister object.</returns>
    public ScatterRegister ReadUInt16(int address, Action<ushort> value) => AddRegistry(
        address, data => value(BitConverter.ToUInt16(data, 0)), sizeof(ushort), null);

    /// <summary>
    /// Reads a uint from the specified address in the register.
    /// </summary>
    /// <param name="address">The address to read the uint from.</param>
    /// <param name="value">The action to perform with the read uint.</param>
    /// <returns>This ScatterRegister object.</returns>
    public ScatterRegister ReadUInt32(int address, Action<uint> value) => AddRegistry(
        address, data => value(BitConverter.ToUInt32(data, 0)), sizeof(uint), null);

    /// <summary>
    /// Reads a ulong from the specified address in the register.
    /// </summary>
    /// <param name="address">The address to read the ulong from.</param>
    /// <param name="value">The action to perform with the read ulong.</param>
    /// <returns>This ScatterRegister object.</returns>
    public ScatterRegister ReadUInt64(int address, Action<ulong> value) => AddRegistry(
        address, data => value(BitConverter.ToUInt64(data, 0)), sizeof(ulong), null);

    /// <summary>
    /// Reads a fixed-point value from the specified address in the register.
    /// </summary>
    /// <param name="totalBits">The total number of bits of the fixed-point value.</param>
    /// <param name="fractionalBits">The number of fractional bits of the fixed-point value.</param>
    /// <param name="address">The address to read the value from.</param>
    /// <param name="value">The action to perform with the read value.</param>
    /// <returns>This ScatterRegister object.</returns>

    public ScatterRegister ReadQAsDouble(int totalBits, int fractionalBits, int address, Action<double> value) => AddRegistry(
        address,
        data =>
        {
            var rawValue = totalBits switch
            {
                8 => data[0],
                16 => BitConverter.ToUInt16(data, 0),
                32 => BitConverter.ToUInt32(data, 0),
                64 => BitConverter.ToUInt64(data, 0),
                _ => throw new ArgumentException($"Unsupported total bits: {totalBits}", nameof(totalBits)),
            };

            var scaleFactor = 1u << fractionalBits;
            value((double)rawValue / scaleFactor);
        }, (byte) (totalBits / 8), null);

    /// <summary>
    /// Reads statistical data from the specified address in the register.
    /// </summary>
    /// <param name="address">The address to read the data from.</param>
    /// <param name="scale">The scale factor to apply to the raw data values.</param>
    /// <param name="statsPeriod">The period of the statistics.</param>
    /// <param name="value">The action to perform with the read statistics.</param>
    /// <returns>This ScatterRegister object.</returns>
    public ScatterRegister ReadStats(int address, double scale, int statsPeriod, Action<Stats> value) => AddRegistry(
        address,
        data =>
        {
            value(
                new Stats()
                {
                    Minimum = BitConverter.ToUInt16(data, 0) * scale,
                    Maximum = BitConverter.ToUInt16(data, 2) * scale,
                    Average = BitConverter.ToUInt32(data, 4) / (double)statsPeriod * scale,
                    StdDeviation = Math.Sqrt(BitConverter.ToUInt32(data, 8) / (double)statsPeriod) * scale,
                });
        }, 12, null);

    /// <summary>
    /// Writes a byte to the specified address in the register.
    /// </summary>
    /// <param name="address">The address to write the byte to.</param>
    /// <param name="value">The byte value to write.</param>
    /// <returns>This ScatterRegister object.</returns>
    public ScatterRegister WriteByte(int address, byte value) => AddRegistry(
        address, null, sizeof(byte), new [] { value });

    /// <summary>
    /// Writes an unsigned 16-bit integer to the specified address in the register.
    /// </summary>
    /// <param name="address">The address to write the integer to.</param>
    /// <param name="value">The integer value to write.</param>
    /// <returns>This ScatterRegister object.</returns>
    public ScatterRegister WriteUInt16(int address, ushort value) => AddRegistry(
        address, null, sizeof(ushort), BitConverter.GetBytes(value));

    /// <summary>
    /// Writes an unsigned 32-bit integer to the specified address in the register.
    /// </summary>
    /// <param name="address">The address to write the integer to.</param>
    /// <param name="value">The integer value to write.</param>
    /// <returns>This ScatterRegister object.</returns>
    public ScatterRegister WriteUInt32(int address, uint value) => AddRegistry(
        address, null, sizeof(uint), BitConverter.GetBytes(value));

    /// <summary>
    /// Writes an unsigned 64-bit integer to the specified address in the register.
    /// </summary>
    /// <param name="address">The address to write the integer to.</param>
    /// <param name="value">The integer value to write.</param>
    /// <returns>This ScatterRegister object.</returns>
    public ScatterRegister WriteUInt64(int address, ulong value) => AddRegistry(
        address, null, sizeof(ulong), BitConverter.GetBytes(value));

    /// <summary>
    /// Writes a fixed-point value to the specified address in the register.
    /// </summary>
    /// <param name="totalBits">The total number of bits of the fixed-point value.</param>
    /// <param name="fractionalBits">The number of fractional bits of the fixed-point value.</param>
    /// <param name="address">The address to write the value to.</param>
    /// <param name="value">The fixed-point value to write.</param>
    /// <returns>This ScatterRegister object.</returns>
    public ScatterRegister WriteQFromDouble(int totalBits, int fractionalBits, int address, double value)
    {
        var scale = Math.Pow(2, fractionalBits);
        var scaledValue = value * scale;

        var bytes = totalBits switch
        {
            8 => new[] { (byte)scaledValue },
            16 => BitConverter.GetBytes((ushort)scaledValue),
            32 => BitConverter.GetBytes((uint)scaledValue),
            64 => BitConverter.GetBytes((ulong)scaledValue),
            _ => throw new ArgumentException($"Unsupported total bits: {totalBits}", nameof(totalBits)),
        };

        return AddRegistry(address, null, (byte)(totalBits / 8), bytes);
    }
}
