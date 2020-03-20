export class Arguments {
    private _isDebug: boolean = false;
    private _isLocal: boolean = false;

    constructor(args: string[]) {
        var markerIndex = args.indexOf('--');
        if (markerIndex >= 0) {
            for (var i = markerIndex + 1; i < args.length; i++) {
                var arg = args[i];
                switch (arg) {
                    case 'debug':
                        this._isDebug = true;
                        break;
                    case 'local':
                        this._isLocal = true;
                        break;
                }
            }
        }
    }

    get isDebug(): boolean {
        return this._isDebug;
    }

    get isLocal(): boolean {
        return this._isLocal;
    }
}
