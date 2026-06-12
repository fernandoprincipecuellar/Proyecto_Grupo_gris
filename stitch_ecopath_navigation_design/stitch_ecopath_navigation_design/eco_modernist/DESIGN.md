---
name: Eco-Modernist
colors:
  surface: '#f5faf9'
  surface-dim: '#d6dbda'
  surface-bright: '#f5faf9'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#f0f5f4'
  surface-container: '#eaefee'
  surface-container-high: '#e4e9e8'
  surface-container-highest: '#dee3e3'
  on-surface: '#171d1c'
  on-surface-variant: '#414847'
  inverse-surface: '#2c3131'
  inverse-on-surface: '#edf2f1'
  outline: '#717977'
  outline-variant: '#c1c8c6'
  surface-tint: '#46645f'
  primary: '#001915'
  on-primary: '#ffffff'
  primary-container: '#0f2e2a'
  on-primary-container: '#779691'
  inverse-primary: '#adcdc7'
  secondary: '#006c50'
  on-secondary: '#ffffff'
  secondary-container: '#00fabe'
  on-secondary-container: '#006f52'
  tertiary: '#021816'
  on-tertiary: '#ffffff'
  tertiary-container: '#162d2b'
  on-tertiary-container: '#7d9592'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#c8e9e3'
  primary-fixed-dim: '#adcdc7'
  on-primary-fixed: '#00201c'
  on-primary-fixed-variant: '#2e4c48'
  secondary-fixed: '#36ffc4'
  secondary-fixed-dim: '#00e1ab'
  on-secondary-fixed: '#002116'
  on-secondary-fixed-variant: '#00513c'
  tertiary-fixed: '#cee8e4'
  tertiary-fixed-dim: '#b3ccc8'
  on-tertiary-fixed: '#081f1d'
  on-tertiary-fixed-variant: '#344b48'
  background: '#f5faf9'
  on-background: '#171d1c'
  surface-variant: '#dee3e3'
typography:
  display-lg:
    fontFamily: Hanken Grotesk
    fontSize: 48px
    fontWeight: '800'
    lineHeight: '1.1'
    letterSpacing: -0.02em
  headline-lg:
    fontFamily: Hanken Grotesk
    fontSize: 32px
    fontWeight: '700'
    lineHeight: '1.2'
  headline-lg-mobile:
    fontFamily: Hanken Grotesk
    fontSize: 24px
    fontWeight: '700'
    lineHeight: '1.2'
  headline-md:
    fontFamily: Hanken Grotesk
    fontSize: 24px
    fontWeight: '600'
    lineHeight: '1.3'
  body-lg:
    fontFamily: Plus Jakarta Sans
    fontSize: 18px
    fontWeight: '400'
    lineHeight: '1.6'
  body-md:
    fontFamily: Plus Jakarta Sans
    fontSize: 16px
    fontWeight: '400'
    lineHeight: '1.6'
  label-md:
    fontFamily: Hanken Grotesk
    fontSize: 14px
    fontWeight: '600'
    lineHeight: '1'
    letterSpacing: 0.05em
rounded:
  sm: 0.25rem
  DEFAULT: 0.5rem
  md: 0.75rem
  lg: 1rem
  xl: 1.5rem
  full: 9999px
spacing:
  unit: 4px
  gutter: 24px
  margin-mobile: 16px
  margin-desktop: 48px
  container-max: 1280px
---

## Brand & Style

The brand personality for this design system is "Eco-Modernist"—a fusion of environmental stewardship and high-tech efficiency. It targets eco-conscious urbanites and community organizers who value sustainability but demand a premium, frictionless digital experience. 

The aesthetic is a sophisticated blend of **Minimalism** and **Glassmorphism**. It utilizes expansive white space and a structured grid to maintain clarity, while layering frosted glass effects and subtle blurs to create a sense of depth and modernity. The UI should evoke a feeling of "Digital Nature": clean, breathing, and technologically advanced yet inherently organic. Transitions should be fluid and eased, mimicking natural movement.

## Colors

The palette is anchored by **Deep Forest Green** (#0F2E2A), used for primary text and structural elements to provide a sense of grounded authority. This is contrasted by **Vibrant Mint** (#00FFC2), a high-energy accent used for interactive states, progress indicators, and "success" messaging.

**Slate Grays** serve as the bridge between these extremes, providing a clean, technical backdrop that prevents the design from feeling overly "earthy." The background uses a soft, cool-tinted neutral to reduce eye strain and enhance the vibrance of the glassmorphism components. Color should be applied with high contrast in mind, ensuring all functional elements remain highly legible against their containers.

## Typography

This design system uses a dual-sans-serif approach to maximize clarity and personality. **Hanken Grotesk** is used for headlines and labels, providing a sharp, technical, and high-contrast look that feels premium and modern. **Plus Jakarta Sans** is used for body text, offering a softer, more approachable feel that remains highly readable at smaller scales.

Typography scales are aggressive to create a clear visual hierarchy. On mobile devices, display and large headline sizes automatically reduce to maintain balance. Heavy weights are preferred for titles to anchor the layout, while body copy maintains a generous line height for a "breathing" editorial quality.

## Layout & Spacing

The layout follows a **Fluid Grid** model with a 12-column structure on desktop, transitioning to a 4-column structure on mobile. A strict 4px/8px baseline rhythm is used to maintain vertical consistency. 

Spacing is intentionally generous to support the minimalist aesthetic. Containers should use dynamic padding that scales with the screen size. On desktop, large side margins create a centered, focused experience, while mobile layouts utilize edge-to-edge components with slightly reduced internal gutters to maximize screen real estate.

## Elevation & Depth

Hierarchy is achieved through **Glassmorphism** and **Tonal Layering**. 

1.  **Base Layer:** Solid, neutral light gray backgrounds.
2.  **Primary Containers:** White surfaces with a high-radius corner and a subtle, extra-diffused "ambient" shadow tinted with the primary forest green (opacity < 5%).
3.  **Accent Containers:** Glassmorphic cards with a background blur of 12px–20px and a 1px semi-transparent white border to simulate light reflecting off an edge.
4.  **Floating Elements:** Elements like "Back to Top" or Chat FABs use a dual-light/dark shadow system to appear physically lifted above the interface.

## Shapes

The shape language is defined by "Soft Modernism." A global border radius of 1rem (16px) is standard for primary cards and containers. Smaller interactive elements like buttons use a slightly reduced radius for a crisper feel, while specific status indicators or secondary tags may use pill shapes (fully rounded) to differentiate them from functional UI blocks. This roundedness mimics organic forms while maintaining a clean, geometric structure.

## Components

### Buttons
Primary buttons use the Forest Green background with Mint text for high-contrast impact. They feature a slight lift on hover and a "pressed" scale effect (0.98x). Secondary buttons use a transparent background with a 2px Mint border.

### Cards (Glassmorphic)
Main dashboard cards utilize a semi-transparent white background with a heavy backdrop blur. This allows background colors or maps to bleed through subtly, creating a premium, layered feel. Cards include a 1px border with a linear gradient (white to transparent) to create a "glass edge."

### Input Fields
Inputs are minimalist, utilizing a solid neutral background that darkens slightly on focus. The active state is indicated by a Mint bottom-border or focus ring. Labels always remain visible, shifting to a smaller uppercase style when the field is active.

### Progress & Visualization
Charts and progress bars exclusively use the Vibrant Mint color. Progress bars should have rounded ends and a subtle glow effect (box-shadow) to emphasize the "energy" of the data.

### Lists & Activity
List items are separated by generous whitespace rather than lines. Each item utilizes a soft-rounded hover state to provide clear feedback. Icons within lists are enclosed in circular containers with low-opacity Mint or Slate backgrounds.