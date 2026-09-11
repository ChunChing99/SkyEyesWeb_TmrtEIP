interface Rgb {
    r: number;
    g: number;
    b: number;
}

const SHADE_STOPS: Record<string, number> = {
    50: -0.95,
    100: -0.9,
    200: -0.75,
    300: -0.6,
    400: -0.3,
    500: 0,
    600: 0.15,
    700: 0.3,
    800: 0.45,
    900: 0.6,
    950: 0.75
};

function normalizeHex(hex: string): string {
    const value = hex.trim().replace(/^#/, '');

    if (value.length === 3) {
        return value
            .split('')
            .map((c) => c + c)
            .join('');
    }

    return value;
}

function hexToRgb(hex: string): Rgb {
    const value = normalizeHex(hex);

    return {
        r: parseInt(value.substring(0, 2), 16),
        g: parseInt(value.substring(2, 4), 16),
        b: parseInt(value.substring(4, 6), 16)
    };
}

function rgbToHex({ r, g, b }: Rgb): string {
    const toHex = (channel: number) =>
        Math.round(Math.min(255, Math.max(0, channel)))
            .toString(16)
            .padStart(2, '0');

    return `#${toHex(r)}${toHex(g)}${toHex(b)}`;
}

function mix(base: Rgb, target: Rgb, ratio: number): Rgb {
    return {
        r: base.r + (target.r - base.r) * ratio,
        g: base.g + (target.g - base.g) * ratio,
        b: base.b + (target.b - base.b) * ratio
    };
}

/**
 * Generates a Tailwind-style 50-950 shade scale from a single base color by
 * mixing it toward white (tints) or black (shades), keeping 500 as the base color itself.
 */
export function generatePalette(baseHex: string): Record<string, string> {
    const base = hexToRgb(baseHex);
    const white: Rgb = { r: 255, g: 255, b: 255 };
    const black: Rgb = { r: 0, g: 0, b: 0 };

    const palette: Record<string, string> = {};

    for (const [stop, ratio] of Object.entries(SHADE_STOPS)) {
        const target = ratio < 0 ? white : black;

        palette[stop] = rgbToHex(mix(base, target, Math.abs(ratio)));
    }

    return palette;
}
