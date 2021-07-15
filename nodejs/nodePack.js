require('./bufferExtension')(Buffer)

let NodePackType = {
    'ConsoleOutput': 0,
    'InvokeCode': 1,
    'InvokeResult': 2,
};

let NodePack = function(id, type, content) {
    this.Id = id;
    this.Type = type;
    this.Content = content;
};

NodePack.prototype.Encode = function() {
    let buff = Buffer.alloc(10 + this.Content.length);
    buff = buff.writeInt32(buff.length-4);
    buff = buff.writeUint8(this.Id);
    buff = buff.writeUint8(this.Type);
    buff = buff.writeString(this.Content);
    return buff;
}

NodePack.Decode = function(buff) {
    var roffset = buff.roffset || 0;
    var length = buff.readInt32();
    if (!length || length <= 0 || length > buff.woffset - buff.roffset) {
        buff.roffset = roffset;
        return null;
    }

    var result = new NodePack(buff.readUint8(), buff.readUint8(), buff.readString());
    buff.clearRead();

    return result;
}

module.exports = {
    "NodePackType": NodePackType,
    "NodePack": NodePack
}

// var buff = Buffer.alloc(1024);
// var data = new NodePack(1, 2, "3").Encode();
// console.log(data);
// console.log(NodePack.Decode(data));