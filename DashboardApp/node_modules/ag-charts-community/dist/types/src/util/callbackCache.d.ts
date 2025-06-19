import { type AnyFn } from 'ag-charts-core';
type Caller = {
    context?: unknown;
};
export declare function callWithContext<F extends AnyFn>(caller1: Caller, caller2: Caller | undefined, fn: F, params: Parameters<F>): ReturnType<F>;
export declare class CallbackCache {
    private cache;
    call<F extends AnyFn>(caller1: Caller, caller2: Caller, fn: F, ...params: Parameters<F>): ReturnType<F> | undefined;
    private invoke;
    invalidateCache(): void;
}
export {};
