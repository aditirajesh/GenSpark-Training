import type { AnyFn } from 'ag-charts-core';
export declare function memo<T, R>(params: T, fnGenerator: (params: T) => () => R): () => R;
export declare function simpleMemorize<F extends AnyFn>(fn: F, cacheCallback?: (type: 'hit' | 'miss', fn: F, keys: any[]) => void): (...p: Parameters<F>) => ReturnType<F>;
