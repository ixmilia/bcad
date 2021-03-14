export class Arguments {
    private _isDebug: boolean = false;
    private _isDevTools: boolean = false;
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
                    case 'devtools':
                        this._isDevTools = true;
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

    get isDevTools(): boolean {
        return this._isDevTools;
    }

    get isLocal(): boolean {
        return this._isLocal;
    }

    getArgList(): string[] {
        const args: string[] = ['--'];
        if (this.isDebug) {
            args.push('debug');
        }

        if (this.isDevTools) {
            args.push('devtools');
        }

        if (this.isLocal) {
            args.push('local');
        }

        return args;
    }
}
