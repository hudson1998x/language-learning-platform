import * as esbuild from "esbuild";
import { sassPlugin } from "esbuild-sass-plugin";
import path from "node:path";
import { fileURLToPath } from "node:url";

const args = process.argv.slice(2);

const indexArg = args.find((arg) => arg.startsWith("--index="));
const watch = args.includes("--watch");

if (!indexArg) {
    console.error("Missing required argument: --index=<path to entry point>");
    process.exit(1);
}

const entryPoint = indexArg.slice("--index=".length);

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const outdir = path.resolve(__dirname, "../web/dist");

/** @type {esbuild.BuildOptions} */
const buildOptions = {
    entryPoints: [entryPoint],
    outdir,
    bundle: true,
    platform: "browser",
    format: "iife",
    target: "es2020",
    jsx: "automatic",
    sourcemap: true,
    loader: {
        ".tsx": "tsx",
        ".ts": "ts",
    },
    plugins: [sassPlugin()],
};

if (watch) {
    const context = await esbuild.context(buildOptions);
    await context.watch();
    console.log(`Watching for changes (entry: ${entryPoint})...`);
} else {
    await esbuild.build(buildOptions);
    console.log(`Build complete (entry: ${entryPoint}) -> ${outdir}`);
}