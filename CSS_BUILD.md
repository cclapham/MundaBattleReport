# CSS Build Setup

The project uses SASS (SCSS) for styling. Styles are organized in `wwwroot/scss/` and compiled to `wwwroot/css/`.

## Quick Setup

### Option 1: Using npm + sass (Recommended)

1. **Install Node.js** if not already installed
   - Download from https://nodejs.org/ (LTS recommended)

2. **Install dependencies:**
   ```bash
   npm install
   ```

3. **Watch for changes (while developing):**
   ```bash
   npm run sass:watch
   ```
   This automatically recompiles CSS whenever you save an SCSS file.

4. **Build for production:**
   ```bash
   npm run sass:build
   ```

### Option 2: Using Visual Studio Web Compiler Extension

1. Install the "Web Compiler" extension in Visual Studio
2. Right-click on `wwwroot/scss/index.scss`
3. Select "Web Compiler" > "Compile" 
4. Enable "Compile on Save" for automatic compilation

### Option 3: Using an Online SASS Compiler (Temporary)

If you don't have Node.js installed yet:
1. Go to https://sass-lang.com/playground
2. Paste your SCSS code
3. Copy the compiled CSS to `wwwroot/css/index.css`

## Project Structure

```
wwwroot/
├── scss/
│   └── index.scss          (Source SASS file)
└── css/
    └── index.css           (Compiled CSS - auto-generated)
```

## File Organization

The SCSS file (`wwwroot/scss/index.scss`) includes:
- Color variables for consistent theming
- Animation keyframes
- Component styles organized logically
- SASS features: variables, nesting, mixins

## Modifying Styles

1. Edit `wwwroot/scss/index.scss`
2. Run `npm run sass:watch` or save (if using Web Compiler)
3. The CSS will automatically compile
4. Refresh your browser to see changes

## CI/CD Integration

The `npm run sass:build` command is configured to run automatically during the build process:
- Development builds: include `npm run sass:build` in pre-build steps
- Production builds: CSS is minified with `--style=compressed`

## Troubleshooting

**CSS not updating?**
- Clear browser cache (Ctrl+Shift+Delete)
- Ensure `npm run sass:watch` is running
- Check the browser console for any CSS errors

**Need to regenerate?**
```bash
npm run sass:build
```

This will recompile all SCSS files in `wwwroot/scss/` to `wwwroot/css/`.
