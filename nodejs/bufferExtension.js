function extension(Buffer) {
    Buffer.prototype.checkAlloc = function(size) {
        this.woffset = this.woffset || 0;
        var needed = this.woffset + size;
        if (this.length >= needed) return this;

        var chunk = Math.max(Buffer.poolSize / 2, 1024);
        var chunkCount = (needed / chunk) >>> 0;
        if ((needed % chunk) > 0) {
            chunkCount += 1;
        }

        var buffer = Buffer.allocUnsafe(chunkCount * chunk);
        buffer.woffset = this.woffset;
        this.copy(buffer, 0, 0, this.woffset);
        return buffer;
    };

    Buffer.prototype.writeUint8 = function (val) {
        var buff = this.checkAlloc(1);
        buff.writeUInt8(val & 0xff, this.woffset);
        this.woffset++;
        return buff;
    }

    Buffer.prototype.writeUint16 = function (val) {
        var buff = this.checkAlloc(2);
        buff.writeUInt16LE(val & 0xffff, buff.woffset);
        buff.woffset += 2;
        return buff;
    }

    Buffer.prototype.writeUint32 = function (val) {
        var buff = this.checkAlloc(4);
        buff.writeUInt32LE(val, buff.woffset);
        buff.woffset += 4;
        return buff;
    }

    Buffer.prototype.writeInt32 = function (val) {
        var buff = this.checkAlloc(4);
        buff.writeInt32LE(val, buff.woffset);
        buff.woffset += 4;
        return buff;
    }

    Buffer.prototype.writeString = function (val) {
        if (!val || val.length <= 0) return this;

        var bytes = Buffer.from(val, 'utf8');
        var buff = this.checkAlloc(4 + bytes.length);

        buff.writeInt32(bytes.length);
        return buff.writeBytes(bytes);
    }

    Buffer.prototype.writeBytes = function (data) {
        if (!data || !data.length) return this;

        var buff = this.checkAlloc(data.length);
        data.copy(buff, this.woffset, 0, data.length);
        buff.woffset = this.woffset + data.length;
        return buff;
    }

    Buffer.prototype.hasReadSize = function (len) {
        this.woffset = this.woffset || 0;
        this.roffset = this.roffset || 0;
        return len <= this.woffset - this.roffset;
    }

    Buffer.prototype.readUint8 = function () {
        if (!this.hasReadSize(1)) return undefined;

        var val = this.readUInt8(this.roffset);
        this.roffset += 1;
        return val;
    }

    Buffer.prototype.readUint16 = function () {
        if (!this.hasReadSize(2)) return undefined;

        var val = this.readUInt16LE(this.roffset);
        this.roffset += 2;
        return val;
    }

    Buffer.prototype.readUint32 = function () {
        if (!this.hasReadSize(4)) return undefined;

        var val = this.readUInt32LE(this.roffset);
        this.roffset += 4;
        return val;
    }

    Buffer.prototype.readInt32 = function () {
        if (!this.hasReadSize(4)) return undefined;

        var val = this.readInt32LE(this.roffset);
        this.roffset += 4;
        return val;
    }

    Buffer.prototype.readBytes = function (len) {
        if (len <= 0) return undefined;

        this.roffset = this.roffset || 0;
        if (this.roffset + len > this.woffset) return undefined;

        var bytes = this.slice(this.roffset, this.roffset + len);
        // console.log(bytes, bytes.length, len);
        this.roffset += len;
        return bytes;
    }

    Buffer.prototype.readString = function () {
        this.roffset = this.roffset || 0;
        var len = this.readInt32();
        if (len <= 0) return undefined;

        var bytes = this.readBytes(len);
        if (bytes == undefined) {
            this.roffset -= 4;
            return undefined;
        }

        return bytes.toString('utf8');
    }

    Buffer.prototype.clearRead = function() {
        if (!this.roffset || !this.woffset) return;

        this.copy(this, 0, this.roffset, this.woffset);
        this.woffset = this.woffset - this.roffset;
        this.roffset = 0;
        
        return this;
    }
}

module.exports = extension;

// extension(Buffer);

// var test = Buffer.alloc(20);
// test.writeUint8(1);
// test.writeUint16(2);
// test.writeUint32(3);
// test.writeInt32(-1);
// console.log(test);

// console.log(test.readUint8());
// console.log(test.readUint16());
// console.log(test.readUint32());
// console.log(test.readInt32());

// test = test.writeString('abc');
// console.log(test);
// var str = test.readString();
// console.log(str);
