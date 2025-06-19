type DestroyFns = {
    push(...args: (() => void)[]): void;
};
export declare function addEscapeEventListener(destroyFns: DestroyFns, elem: HTMLElement, onEscape: (event: KeyboardEvent) => void, keyCodes?: readonly string[]): void;
export declare function addMouseCloseListener(destroyFns: DestroyFns, menu: HTMLElement, hideCallback: () => void): () => void;
export declare function addTouchCloseListener(destroyFns: DestroyFns, menu: HTMLElement, hideCallback: () => void): () => void;
export declare function addOverrideFocusVisibleEventListener(destroyFns: DestroyFns, menu: HTMLElement, buttons: HTMLElement[], overrideFocusVisible: boolean): void;
export declare function hasNoModifiers(event: KeyboardEvent | MouseEvent): boolean;
export declare const PREV_NEXT_KEYS: {
    readonly horizontal: {
        readonly nextKey: "ArrowRight";
        readonly prevKey: "ArrowLeft";
    };
    readonly vertical: {
        readonly nextKey: "ArrowDown";
        readonly prevKey: "ArrowUp";
    };
};
export declare function initRovingTabIndex(opts: {
    orientation: 'horizontal' | 'vertical';
    buttons: HTMLElement[];
    wrapAround?: boolean;
    onFocus?: (event: FocusEvent) => void;
    onBlur?: (event: FocusEvent) => void;
    onEscape?: (event: KeyboardEvent) => void;
}): (() => void)[];
export interface MenuCloser {
    close(): void;
    finishClosing(): void;
}
export declare function initMenuKeyNav(opts: {
    orientation: 'vertical';
    sourceEvent: Event;
    menu: HTMLElement;
    buttons: HTMLElement[];
    overrideFocusVisible?: false;
    closeCallback: () => void;
}): MenuCloser;
export declare function makeAccessibleClickListener(element: HTMLElement, onclick: (event: MouseEvent) => unknown): (event: MouseEvent) => void;
export declare function isButtonClickEvent(event: KeyboardEvent | MouseEvent): boolean;
export declare function getLastFocus(sourceEvent: Event | undefined): HTMLElement | undefined;
export declare function stopPageScrolling(element: HTMLElement): () => void;
export {};
