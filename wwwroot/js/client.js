(function (exports, ByteArray) {
    /* defines */
    var client = {};
    var pkg = {
        "ConsoleOutput" : 0x00,
        "InvokeCode"    : 0x01,
        "InvokeResult"  : 0x02
    };
    var events = {
        "CONNECTED"    : "__ON_CONNECTED",
        "DISCONNECTED" : "__ON_DISCONNECTED",
        "ERROR" : "__ON_ERROR"
    };

    var ws = null; //websocket Object

    /**
     * Inherit the emitter properties.
     *
     * @param {Object} obj
     * @param {Object} superCls
     * @return {Object}
     * @api private
     */
    function inherit(obj, superCls) {
        for (var key in superCls.prototype) {
            obj[key] = superCls.prototype[key];
        }
        return obj;
    }

    function strencode(str) {
        var byteArray = new ByteArray(str.length * 3);
        var offset = 0;
        for (var i = 0; i < str.length; i++) {
            var charCode = str.charCodeAt(i);
            var codes = null;
            if (charCode <= 0x7f) {
                codes = [charCode];
            } else if (charCode <= 0x7ff) {
                codes = [0xc0 | (charCode >> 6), 0x80 | (charCode & 0x3f)];
            } else {
                codes = [0xe0 | (charCode >> 12), 0x80 | ((charCode & 0xfc0) >> 6), 0x80 | (charCode & 0x3f)];
            }
            for (var j = 0; j < codes.length; j++) {
                byteArray[offset] = codes[j];
                ++offset;
            }
        }
        var _buffer = new ByteArray(offset);
        copyArray(_buffer, 0, byteArray, 0, offset);
        return _buffer;
    };

    function strdecode(buffer) {
        var bytes = new ByteArray(buffer);
        var array = [];
        var offset = 0;
        var charCode = 0;
        var end = bytes.length;
        while (offset < end) {
            if (bytes[offset] < 128) {
                charCode = bytes[offset];
                offset += 1;
            } else if (bytes[offset] < 224) {
                charCode = ((bytes[offset] & 0x3f) << 6) + (bytes[offset + 1] & 0x3f);
                offset += 2;
            } else {
                charCode = ((bytes[offset] & 0x0f) << 12) + ((bytes[offset + 1] & 0x3f) << 6) + (bytes[offset + 2] & 0x3f);
                offset += 3;
            }
            array.push(charCode);
        }
        return String.fromCharCode.apply(null, array);
    };

    function copyArray(dest, doffset, src, soffset, length) {
        if ('function' === typeof src.copy) {
            // Buffer
            src.copy(dest, doffset, soffset, soffset + length);
            return dest;
        } else {
            // Uint8Array
            var result = dest;
            if (dest.length < (doffset + length)) {
                result = new ByteArray(doffset + length);
            }

            for (var i = 0; i < dest.length; i++) {
                result[i] = dest[i];
            }

            for (var index = 0; index < length; index++) {
                result[doffset++] = src[soffset++];
            }

            return result;
        }
    }

    function IdGen(max) {
        this.id = 0;
        this.max = max;
        return this;
    }

    IdGen.prototype.next = function () {
        if (this.id++ > this.max) {
            this.id = 0;
        }

        return this.id;
    }

    ByteArray.prototype.writeUint8 = function (val) {
        this.woffset = this.woffset || 0;
        this[this.woffset++] = val & 0xff;
        return this;
    }

    ByteArray.prototype.writeUint16 = function (val) {
        this.woffset = this.woffset || 0;
        
        this[this.woffset++] = val & 0xff;
        this[this.woffset++] = (val >> 8) & 0xff;

        return this;
    }

    ByteArray.prototype.writeUint32 = function (val) {
        this.woffset = this.woffset || 0;

        this[this.woffset++] = val & 0xff;
        this[this.woffset++] = (val >> 8) & 0xff;
        this[this.woffset++] = (val >> 16) & 0xff;
        this[this.woffset++] = (val >> 24) & 0xff;
        
        return this;
    }

    ByteArray.prototype.writeString = function (val) {
        if (!val || val.length <= 0) return this;

        this.woffset = this.woffset || 0;
        var bytes = strencode(val);
        // console.log(val, bytes, bytes.length); 
        var result = copyArray(this, this.woffset, bytes, 0, bytes.length);
        result.woffset = this.woffset + bytes.length;
        return result;
    }

    ByteArray.prototype.writeBytes = function (data) {
        if (!data || !data.length) return this;

        var result = copyArray(this, this.woffset, data, 0, data.length);
        this.woffset = this.woffset || 0;
        result.roffset = this.roffset || 0;
        result.woffset = this.woffset + data.length;
        return result;
    }

    ByteArray.prototype.hasReadSize = function (len) {
        this.roffset = this.roffset || 0;
        this.woffset = this.woffset || this.length;
        return len <= this.woffset - this.roffset;
    }

    ByteArray.prototype.readUint8 = function () {
        this.roffset = this.roffset || 0;
        if (this.roffset + 1 > this.woffset) return undefined;

        var val = this[this.roffset] & 0xff;
        this.roffset += 1;
        return val;
    }

    ByteArray.prototype.readUint16 = function () {
        var l = this.readUint8();
        var h = this.readUint8();
        if (h === undefined || l === undefined) return undefined;

        return h << 8 | l;
    }

    ByteArray.prototype.readUint32 = function () {
        var b3 = this.readUint8();
        var b2 = this.readUint8();
        var b1 = this.readUint8();
        var b0 = this.readUint8();
        if (b0 === undefined || b1 === undefined || b2 === undefined || b3 === undefined) return undefined;

        return b0 << 24 | b1 << 16 | b2 << 8 | b3;
    }

    ByteArray.prototype.readBytes = function (len) {
        if (len <= 0) return undefined;

        this.roffset = this.roffset || 0;
        if (this.roffset + len > this.woffset) return undefined;

        var bytes = this.slice(this.roffset, this.roffset + len);
        // console.log(bytes, bytes.length, len);
        this.roffset += len;
        return bytes;
    }

    ByteArray.prototype.readString = function (len) {
        var bytes = this.readBytes(len);
        if (bytes == undefined) return "";

        return strdecode(bytes);
    }

    ByteArray.prototype.clearRead = function() {
        copyArray(this, 0, this, this.roffset, this.woffset-this.roffset);
        this.woffset = this.woffset - this.roffset;
        this.roffset = 0;
    }
    /* ^^^^^^ Utility Functions End ^^^^^^ */

    /* vvvvvv Encoder Start vvvvvv */
    var jsonEncoder = {
        "encode": function(obj) {
            if(obj == undefined) return obj;

            obj = JSON.stringify(obj);
            return strencode(obj);
        },
        "decode": function(buffer) {
            buffer = strdecode(buffer);
            return JSON.parse(buffer);
        }
    };

    function GetEncoder() {
        return jsonEncoder;
    }
    /* ^^^^^^ Encoder End ^^^^^^ */

    /* vvvvvv Event Emitter Start vvvvvv */
    function Emitter(obj) {
        if (obj) return inherit(obj, Emitter);
    }

    /**
     * Listen on the given `event` with `fn`.
     *
     * @param {String} event
     * @param {Function} fn
     * @return {Emitter}
     * @api public
     */
    Emitter.prototype.on =
        Emitter.prototype.addListener =
        Emitter.prototype.addEventListener = function (event, fn) {
            this._callbacks = this._callbacks || {};
            (this._callbacks[event] = this._callbacks[event] || [])
            .push(fn);
            return this;
        };

    /**
     * Adds an `event` listener that will be invoked a single
     * time then automatically removed.
     *
     * @param {String} event
     * @param {Function} fn
     * @return {Emitter}
     * @api public
     */
    Emitter.prototype.once = function (event, fn) {
        var self = this;
        this._callbacks = this._callbacks || {};

        function on() {
            self.off(event, on);
            fn.apply(this, arguments);
        }

        on.fn = fn;
        this.on(event, on);
        return this;
    };

    /**
     * Remove the given callback for `event` or all
     * registered callbacks.
     *
     * @param {String} event
     * @param {Function} fn
     * @return {Emitter}
     * @api public
     */
    Emitter.prototype.off =
        Emitter.prototype.removeListener =
        Emitter.prototype.removeAllListeners =
        Emitter.prototype.removeEventListener = function (event, fn) {
            this._callbacks = this._callbacks || {};

            // all
            if (0 == arguments.length) {
                this._callbacks = {};
                return this;
            }

            // specific event
            var callbacks = this._callbacks[event];
            if (!callbacks) return this;

            // remove all handlers
            if (1 == arguments.length) {
                delete this._callbacks[event];
                return this;
            }

            // remove specific handler
            var cb;
            for (var i = 0; i < callbacks.length; i++) {
                cb = callbacks[i];
                if (cb === fn || cb.fn === fn) {
                    callbacks.splice(i, 1);
                    break;
                }
            }
            return this;
        };

    /**
     * Emit `event` with the given args.
     *
     * @param {String} event
     * @param {Mixed} ...
     * @return {Emitter}
     */
    Emitter.prototype.emit = function (event) {
        this._callbacks = this._callbacks || {};
        var args = [].slice.call(arguments, 1),
            callbacks = this._callbacks[event];

        if (callbacks) {
            callbacks = callbacks.slice(0);
            for (var i = 0, len = callbacks.length; i < len; ++i) {
                callbacks[i].apply(this, args);
            }
        }

        return this;
    };

    /**
     * Return array of callbacks for `event`.
     *
     * @param {String} event
     * @return {Array}
     * @api public
     */
    Emitter.prototype.listeners = function (event) {
        this._callbacks = this._callbacks || {};
        return this._callbacks[event] || [];
    };

    /**
     * Check if this emitter has `event` handlers.
     *
     * @param {String} event
     * @return {Boolean}
     * @api public
     */
    Emitter.prototype.hasListeners = function (event) {
        return !!this.listeners(event).length;
    };
    /* ^^^^^^^ Event Emitter End ^^^^^^^ */

    client.connect = function (host, port) {
        var url = "ws://" + host + ":" + port;
        if (client.isConnected() && client.url == url) return;

        if (client.isConnected() && client.url != url) client.disconnect();

        client.url = url;
        ws = new WebSocket(url);
        ws.binaryType = 'arraybuffer';
        ws.onopen = client.onopen;
        ws.onmessage = client.onmessage;
        ws.onerror = client.onerror;
        ws.onclose = client.onclose;
    }

    client.disconnect = function () {
        if (!ws) return;

        if (ws.readyState <= 1) ws.close();
        ws = null;
    }

    client.isConnected = function () {
        if (!ws) return false;
        if (ws.readyState > 1) return false;

        return true;
    }

    client.send = function (pack) {
        if (!pack) return;

        var data = new ByteArray(10+pack.Content.length);
        data.writeUint32(data.length - 4);
        data.writeUint8(pack.Id);
        data.writeUint8(pack.Type);
        data.writeUint32(pack.Content.length);
        data.writeString(pack.Content);

        // console.log("send:", data);
        ws.send(data);
    }

    client.recv = function () {
        if (!client.buffer || !client.buffer.length) return null;

        var size = client.buffer.readUint32();
        // console.log("size:", size);
        if (!size) return null;
        if (!client.buffer.hasReadSize(size)) {
            client.buffer.roffset -= 4;
            return null;
        }

        var data = client.buffer.readBytes(size);
        var id = data.readUint8();
        var type = data.readUint8();
        var contentSize = data.readUint32();
        var content = data.readString(contentSize);

        client.buffer.clearRead();
        // console.log("recved:", id, type, contentSize, content);
        return {
            "Id": id,
            "Type": type,
            "Content": content
        };
    }

    client.onopen = function (event) {
        console.log("onopen", event)
        client.emit(events.CONNECTED, event);
    }

    client.onmessage = function (event) {
        var data = new ByteArray(event.data);
        // console.log("onmessage", event, data);

        if (!client.buffer) {
            client.buffer = data;
        } else {
            client.buffer = client.buffer.writeBytes(data);
        }

        var pack = client.recv();
        // console.log("recv pack:", pack);
        while (pack) {
            switch (pack.Type) {
                case pkg.ConsoleOutput:
                    console.log('[CS] ' + pack.Content);
                    break;
                case pkg.InvokeCode:
                    client.onInvoke(pack);
                    break;
                case pkg.InvokeResult:
                    client.onResult(pack);
                    break;
            }

            pack = client.recv();
            if (!pack) return;
        }
    }

    client.onerror = function (event) {
        console.log("onerror", event);
        client.emit(events.ERROR, event);
    }

    client.onclose = function (event) {
        console.log("onclose", event);
        client.emit(events.DISCONNECTED, event);
        client.disconnect();
    }

    client.onInvoke = function (pack) {
        console.log("onInvke", pack);
        var result = eval(pack.Content);
        console.log(pack.Content, result);
        result = result === undefined ? null : result;
        client.send({
            "Id": pack.Id,
            "Type": pkg.InvokeResult,
            "Content": JSON.stringify(result)
        });
    }

    client.onResult = function (pack) {
        console.log("onResult", pack);
        var data = JSON.parse(pack.Content);
        if (data && data.err) console.error(data.err);

        client.emit(client.getCallbackKey(pack.Id), data);
    }

    client.getCallbackKey = function (id) {
        return "invoke-" + id;
    }

    client.invoke = function(cls, method) {
        var args = [];
        if (arguments.length > 2) args = [].slice.call(arguments, 2);

        var encoder = GetEncoder();
        client.idGen = client.idGen || new IdGen(255);
        var pack = {
            "Class": cls,
            "Method": method,
            "Arguments": args
        };

        pack = {
            "Id": client.idGen.next(),
            "Type": pkg.InvokeCode,
            "Content": JSON.stringify(pack)
        };
        // console.log('invoke', pack);
        client.send(pack);

        return new Promise(function(resolve, reject) {
            var id = pack.Id;
            client.once(client.getCallbackKey(id), resolve);
        });
    }

    client.invokeNode = function(code, resultCsType="object") {
        return client.invoke("ElectronFlex.NodeJs", "Invoke<" + resultCsType + ">", code);
    }

    client = Emitter(client);
    client.pkg = pkg;
    client.events = events;

    exports.Emitter = Emitter;
    exports.client = client;
})(
    typeof (window) == "undefined" ? module.exports : window,
    typeof (window) == "undefined" ? Buffer : Uint8Array
)