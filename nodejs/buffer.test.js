require('./bufferExtension')(Buffer);

test('Uint8', () => {
    var buff = Buffer.alloc(2);
    expect(buff.length).toBe(2);

    buff = buff.writeUint8(1);
    expect(buff.toArray()).toStrictEqual([1]);
    expect(buff.woffset).toBe(1);
    
    buff = buff.writeUint8(2);
    expect(buff.toArray()).toStrictEqual([1, 2]);
    expect(buff.woffset).toBe(2);

    expect(buff.length).toBe(2);

    expect(buff.readUint8()).toBe(1);
    expect(buff.roffset).toBe(1);
    expect(buff.readUint8()).toBe(2);
    expect(buff.roffset).toBe(2);
    expect(buff.readUint8()).toBe(undefined);
});

test('Uint16', () => {
    var buff = Buffer.alloc(4);
    expect(buff.length).toBe(4);

    buff = buff.writeUint16(1);
    expect(buff.toArray()).toStrictEqual([1, 0]);
    expect(buff.woffset).toBe(2);
    
    buff = buff.writeUint16(2);
    expect(buff.toArray()).toStrictEqual([1, 0, 2, 0]);
    expect(buff.woffset).toBe(4);

    expect(buff.length).toBe(4);

    expect(buff.readUint16()).toBe(1);
    expect(buff.roffset).toBe(2);
    expect(buff.readUint16()).toBe(2);
    expect(buff.roffset).toBe(4);
    expect(buff.readUint16()).toBe(undefined);
});

test('Uint32', () => {
    var buff = Buffer.alloc(8);
    expect(buff.length).toBe(8);

    buff = buff.writeUint32(1);
    expect(buff.toArray()).toStrictEqual([1, 0, 0, 0]);
    expect(buff.woffset).toBe(4);
    
    buff = buff.writeUint32(2);
    expect(buff.toArray()).toStrictEqual([1, 0, 0, 0, 2, 0, 0, 0]);
    expect(buff.woffset).toBe(8);

    expect(buff.length).toBe(8);

    expect(buff.readUint32()).toBe(1);
    expect(buff.roffset).toBe(4);
    expect(buff.readUint32()).toBe(2);
    expect(buff.roffset).toBe(8);
    expect(buff.readUint32()).toBe(undefined);
});

test('Int32', () => {
    var buff = Buffer.alloc(8);
    expect(buff.length).toBe(8);

    buff = buff.writeInt32(1);
    expect(buff.toArray()).toStrictEqual([1, 0, 0, 0]);
    expect(buff.woffset).toBe(4);
    
    buff = buff.writeInt32(2);
    expect(buff.toArray()).toStrictEqual([1, 0, 0, 0, 2, 0, 0, 0]);
    expect(buff.woffset).toBe(8);

    expect(buff.length).toBe(8);

    expect(buff.readInt32()).toBe(1);
    expect(buff.roffset).toBe(4);
    expect(buff.readInt32()).toBe(2);
    expect(buff.roffset).toBe(8);
    expect(buff.readInt32()).toBe(undefined);
});

test('Bytes', () => {
    var buff = Buffer.alloc(8);
    expect(buff.length).toBe(8);

    buff = buff.writeBytes([1, 2, 3]);
    expect(buff.toArray()).toStrictEqual([1, 2, 3]);
    expect(buff.woffset).toBe(3);
    
    buff = buff.writeBytes(Buffer.from([4, 5]));
    expect(buff.toArray()).toStrictEqual([1, 2, 3, 4, 5]);
    expect(buff.woffset).toBe(5);

    expect(buff.length).toBe(8);

    expect(buff.readBytes(1).toArray()).toStrictEqual([1]);
    expect(buff.roffset).toBe(1);
    expect(buff.readBytes(2).toArray()).toStrictEqual([2, 3]);
    expect(buff.roffset).toBe(3);
    expect(buff.readBytes(3)).toBe(undefined);
    expect(buff.roffset).toBe(3);
    expect(buff.readBytes(2).toArray()).toStrictEqual([4, 5]);
    expect(buff.roffset).toBe(5);
});

test('String', () => {
    var buff = Buffer.alloc(12);
    expect(buff.length).toBe(12);

    buff = buff.writeString('ab');
    expect(buff.toArray()).toStrictEqual([2, 0, 0, 0, 97, 98]);
    expect(buff.woffset).toBe(6);
    
    buff = buff.writeString('cd');
    expect(buff.toArray()).toStrictEqual([2, 0, 0, 0, 97, 98, 2, 0, 0, 0, 99, 100]);
    expect(buff.woffset).toBe(12);

    expect(buff.length).toBe(12);

    expect(buff.readString()).toBe('ab');
    expect(buff.roffset).toBe(6);
    expect(buff.readString()).toBe('cd');
    expect(buff.roffset).toBe(12);
    expect(buff.readString()).toBe(undefined);
    expect(buff.roffset).toBe(12);
});