"""
ShareMeal Platform — Professional FYP Presentation Generator
University of Southern Punjab, Multan
Supervisor: Prof. Kinat
Developers: Muhammad Khulfan & Abdullah Khalid
"""

from pptx import Presentation
from pptx.util import Inches, Pt, Emu
from pptx.dml.color import RGBColor
from pptx.enum.text import PP_ALIGN
from pptx.enum.dml import MSO_THEME_COLOR
from pptx.oxml import parse_xml
import copy, io, os, math

# ── Output path ──────────────────────────────────────────────────
OUTPUT = os.path.join(os.path.dirname(__file__), "wwwroot", "ShareMeal_Presentation.pptx")

# ── Brand Colors ─────────────────────────────────────────────────
DARK_BG     = RGBColor(0x02, 0x06, 0x17)      # #020617 deep navy
CARD_BG     = RGBColor(0x0D, 0x1B, 0x2A)      # #0D1B2A
EMERALD     = RGBColor(0x10, 0xB9, 0x81)      # #10b981
EMERALD_DK  = RGBColor(0x05, 0x96, 0x69)      # #059669
BLUE_ACC    = RGBColor(0x3B, 0x82, 0xF6)      # #3b82f6
PURPLE_ACC  = RGBColor(0xA8, 0x55, 0xF7)      # #a855f7
GOLD_ACC    = RGBColor(0xF5, 0x9E, 0x0B)      # #f59e0b
WHITE       = RGBColor(0xFF, 0xFF, 0xFF)
GRAY_400    = RGBColor(0x9C, 0xA3, 0xAF)
GRAY_600    = RGBColor(0x4B, 0x55, 0x63)
RED_ACC     = RGBColor(0xEF, 0x44, 0x44)

SLIDE_W = Inches(13.33)
SLIDE_H = Inches(7.5)

prs = Presentation()
prs.slide_width  = SLIDE_W
prs.slide_height = SLIDE_H

BLANK = prs.slide_layouts[6]   # completely blank layout

# ═══════════════════════════════════════════════════════════════════
# HELPERS
# ═══════════════════════════════════════════════════════════════════

def add_rect(slide, x, y, w, h, fill=None, line_color=None, line_w=Pt(0), radius=0):
    shape = slide.shapes.add_shape(1, x, y, w, h)          # MSO_SHAPE_TYPE.RECTANGLE = 1
    shape.line.width = line_w
    if fill:
        shape.fill.solid()
        shape.fill.fore_color.rgb = fill
    else:
        shape.fill.background()
    if line_color:
        shape.line.color.rgb = line_color
    else:
        shape.line.fill.background()
    return shape

def add_text(slide, text, x, y, w, h,
             font_size=Pt(14), bold=False, italic=False,
             color=WHITE, align=PP_ALIGN.LEFT,
             word_wrap=True, font_name="Calibri"):
    tb = slide.shapes.add_textbox(x, y, w, h)
    tf = tb.text_frame
    tf.word_wrap = word_wrap
    p = tf.paragraphs[0]
    p.alignment = align
    run = p.add_run()
    run.text = text
    run.font.size = font_size
    run.font.bold = bold
    run.font.italic = italic
    run.font.color.rgb = color
    run.font.name = font_name
    return tb

def add_slide(title_text="", transition="fade"):
    slide = prs.slides.add_slide(BLANK)
    # Full dark background
    add_rect(slide, 0, 0, SLIDE_W, SLIDE_H, fill=DARK_BG)
    # Add native PowerPoint slide transition (animates when F5 pressed in PPT)
    try:
        if transition == "push":
            trans_xml = parse_xml('<p:transition xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main" spd="med"><p:push dir="l"/></p:transition>')
        elif transition == "wipe":
            trans_xml = parse_xml('<p:transition xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main" spd="med"><p:wipe/></p:transition>')
        else:
            trans_xml = parse_xml('<p:transition xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main" spd="med"><p:fade/></p:transition>')
        slide._element.append(trans_xml)
    except Exception as e:
        pass
    return slide

def accent_bar(slide, color=EMERALD, w=Inches(1.2), h=Inches(0.06)):
    """Horizontal accent line under section label."""
    add_rect(slide, Inches(0.55), Inches(1.38), w, h, fill=color)

def section_label(slide, label, color=EMERALD):
    add_text(slide, label.upper(), Inches(0.55), Inches(1.0), Inches(6), Inches(0.35),
             font_size=Pt(10), bold=True, color=color, font_name="Calibri")
    accent_bar(slide, color)

def gradient_header_bar(slide, color1=EMERALD, color2=EMERALD_DK):
    bar = add_rect(slide, 0, 0, SLIDE_W, Inches(0.08), fill=color1)
    return bar

def card(slide, x, y, w, h, bg=CARD_BG, border=None):
    r = add_rect(slide, x, y, w, h, fill=bg, line_color=border, line_w=Pt(1.2))
    return r

def bullet_point(slide, icon, text, x, y, w, icon_color=EMERALD, text_size=Pt(13)):
    add_text(slide, icon, x, y, Inches(0.4), Inches(0.35),
             font_size=text_size, color=icon_color, align=PP_ALIGN.CENTER, font_name="Segoe UI Emoji")
    add_text(slide, text, x + Inches(0.42), y, w - Inches(0.42), Inches(0.35),
             font_size=text_size, color=GRAY_400, font_name="Calibri")

def divider(slide, y, color=GRAY_600, alpha_factor=0.3):
    w_line = Inches(12.2)
    x_line = Inches(0.55)
    add_rect(slide, x_line, y, w_line, Inches(0.01), fill=color)


# ═══════════════════════════════════════════════════════════════════
# SLIDE 1 — TITLE
# ═══════════════════════════════════════════════════════════════════
s1 = add_slide()

# Side accent strip
add_rect(s1, 0, 0, Inches(0.18), SLIDE_H, fill=EMERALD)

# Large background icon (faded)
add_text(s1, "🍱", Inches(8.5), Inches(1.2), Inches(4), Inches(4),
         font_size=Pt(200), color=RGBColor(0x0A, 0x2A, 0x1E), align=PP_ALIGN.CENTER, font_name="Segoe UI Emoji")

# University tag at top
add_text(s1, "UNIVERSITY OF SOUTHERN PUNJAB, MULTAN", Inches(0.55), Inches(0.22),
         Inches(9), Inches(0.35), font_size=Pt(9), bold=True, color=EMERALD,
         font_name="Calibri", align=PP_ALIGN.LEFT)

# Main title
add_text(s1, "ShareMeal", Inches(0.55), Inches(1.2), Inches(8), Inches(1.5),
         font_size=Pt(72), bold=True, color=EMERALD, font_name="Calibri", align=PP_ALIGN.LEFT)
add_text(s1, "Platform", Inches(0.55), Inches(2.5), Inches(8), Inches(1.1),
         font_size=Pt(64), bold=True, color=WHITE, font_name="Calibri", align=PP_ALIGN.LEFT)

# Tagline
add_text(s1, "Connecting Food.  Reducing Waste.  Saving Lives.",
         Inches(0.55), Inches(3.5), Inches(9), Inches(0.5),
         font_size=Pt(16), italic=True, color=GRAY_400, font_name="Calibri", align=PP_ALIGN.LEFT)

# Divider
add_rect(s1, Inches(0.55), Inches(4.15), Inches(5.5), Inches(0.04), fill=EMERALD)

# Info block
info_y = Inches(4.45)
gap = Inches(0.38)
add_text(s1, "Final Year Project (FYP)",
         Inches(0.55), info_y, Inches(7), Inches(0.36), font_size=Pt(13), bold=True, color=WHITE, font_name="Calibri")
add_text(s1, "Supervisor:  Prof. Kinat",
         Inches(0.55), info_y + gap, Inches(7), Inches(0.36), font_size=Pt(13), color=GRAY_400, font_name="Calibri")
add_text(s1, "Developers:  Muhammad Khulfan   |   Abdullah Khalid",
         Inches(0.55), info_y + gap*2, Inches(9), Inches(0.36), font_size=Pt(13), color=GRAY_400, font_name="Calibri")
add_text(s1, "BSCS  —  University of Southern Punjab, Multan  —  2026",
         Inches(0.55), info_y + gap*3, Inches(9), Inches(0.36), font_size=Pt(12), color=GRAY_600, font_name="Calibri")

# Bottom strip
add_rect(s1, 0, SLIDE_H - Inches(0.08), SLIDE_W, Inches(0.08), fill=EMERALD)


# ═══════════════════════════════════════════════════════════════════
# SLIDE 2 — TABLE OF CONTENTS
# ═══════════════════════════════════════════════════════════════════
s2 = add_slide()
add_rect(s2, 0, 0, Inches(0.18), SLIDE_H, fill=BLUE_ACC)
gradient_header_bar(s2, BLUE_ACC, BLUE_ACC)

add_text(s2, "TABLE OF CONTENTS", Inches(0.4), Inches(0.22),
         Inches(9), Inches(0.35), font_size=Pt(9), bold=True, color=BLUE_ACC, font_name="Calibri")

add_text(s2, "Presentation Overview", Inches(0.55), Inches(0.7),
         Inches(9), Inches(0.6), font_size=Pt(28), bold=True, color=WHITE, font_name="Calibri")

add_rect(s2, Inches(0.55), Inches(1.3), Inches(1.8), Inches(0.05), fill=BLUE_ACC)

toc_items = [
    ("01", "Problem Statement",       "Food waste crisis in Pakistan",          EMERALD),
    ("02", "Solution Overview",       "ShareMeal Platform — what it does",       BLUE_ACC),
    ("03", "System Architecture",     "MVC pattern, flow diagram",               PURPLE_ACC),
    ("04", "Key Features",            "8 major features explained",              GOLD_ACC),
    ("05", "Technology Stack",        "ASP.NET Core, SQLite, Gemini AI",         EMERALD),
    ("06", "Database Design",         "Models, tables, relationships",           BLUE_ACC),
    ("07", "AI Integration",          "MealBot — Google Gemini 2.0 Flash",       PURPLE_ACC),
    ("08", "Live Deployment",         "Docker + Railway.app + CI/CD",            GOLD_ACC),
    ("09", "Team & Supervisor",       "Muhammad Khulfan, Abdullah Khalid, Prof. Kinat", EMERALD),
]

col1 = toc_items[:5]
col2 = toc_items[5:]

for i, (num, title, sub, clr) in enumerate(col1):
    y = Inches(1.55) + i * Inches(0.96)
    card(s2, Inches(0.5), y, Inches(5.8), Inches(0.82), bg=CARD_BG, border=clr)
    add_text(s2, num, Inches(0.62), y + Inches(0.12), Inches(0.55), Inches(0.55),
             font_size=Pt(20), bold=True, color=clr, font_name="Calibri")
    add_text(s2, title, Inches(1.28), y + Inches(0.08), Inches(4.8), Inches(0.35),
             font_size=Pt(14), bold=True, color=WHITE, font_name="Calibri")
    add_text(s2, sub, Inches(1.28), y + Inches(0.44), Inches(4.8), Inches(0.3),
             font_size=Pt(10), color=GRAY_400, font_name="Calibri")

for i, (num, title, sub, clr) in enumerate(col2):
    y = Inches(1.55) + i * Inches(0.96)
    card(s2, Inches(6.9), y, Inches(5.8), Inches(0.82), bg=CARD_BG, border=clr)
    add_text(s2, num, Inches(7.02), y + Inches(0.12), Inches(0.55), Inches(0.55),
             font_size=Pt(20), bold=True, color=clr, font_name="Calibri")
    add_text(s2, title, Inches(7.68), y + Inches(0.08), Inches(4.8), Inches(0.35),
             font_size=Pt(14), bold=True, color=WHITE, font_name="Calibri")
    add_text(s2, sub, Inches(7.68), y + Inches(0.44), Inches(4.8), Inches(0.3),
             font_size=Pt(10), color=GRAY_400, font_name="Calibri")

add_rect(s2, 0, SLIDE_H - Inches(0.08), SLIDE_W, Inches(0.08), fill=BLUE_ACC)


# ═══════════════════════════════════════════════════════════════════
# SLIDE 3 — PROBLEM STATEMENT
# ═══════════════════════════════════════════════════════════════════
s3 = add_slide()
add_rect(s3, 0, 0, Inches(0.18), SLIDE_H, fill=RED_ACC)
gradient_header_bar(s3, RED_ACC, RED_ACC)

add_text(s3, "01  ·  PROBLEM STATEMENT", Inches(0.4), Inches(0.22),
         Inches(9), Inches(0.35), font_size=Pt(9), bold=True, color=RED_ACC, font_name="Calibri")
add_text(s3, "Pakistan's Food Crisis", Inches(0.55), Inches(0.7),
         Inches(9), Inches(0.6), font_size=Pt(32), bold=True, color=WHITE, font_name="Calibri")
add_rect(s3, Inches(0.55), Inches(1.32), Inches(2.5), Inches(0.05), fill=RED_ACC)

# 3 stat cards
stats = [
    ("36%",  "Food Wasted",        "Of all food produced in Pakistan is wasted annually", "🗑️", RED_ACC),
    ("2 Cr+","People Hungry",      "People sleep hungry every night in Pakistan",          "😢", GOLD_ACC),
    ("70%",  "Restaurants Waste",  "Restaurants throw away leftover food daily",           "🍽️", PURPLE_ACC),
]
for i, (num, title, desc, icon, clr) in enumerate(stats):
    x = Inches(0.55) + i * Inches(4.2)
    card(s3, x, Inches(1.55), Inches(3.9), Inches(2.8), bg=CARD_BG, border=clr)
    add_text(s3, icon, x + Inches(0.15), Inches(1.7), Inches(0.9), Inches(0.9),
             font_size=Pt(38), color=clr, align=PP_ALIGN.CENTER, font_name="Segoe UI Emoji")
    add_text(s3, num, x + Inches(1.1), Inches(1.72), Inches(2.6), Inches(0.75),
             font_size=Pt(44), bold=True, color=clr, font_name="Calibri")
    add_text(s3, title, x + Inches(0.2), Inches(2.52), Inches(3.5), Inches(0.38),
             font_size=Pt(15), bold=True, color=WHITE, font_name="Calibri")
    add_text(s3, desc, x + Inches(0.2), Inches(2.92), Inches(3.5), Inches(0.85),
             font_size=Pt(11), color=GRAY_400, font_name="Calibri", word_wrap=True)

# Root cause
card(s3, Inches(0.55), Inches(4.55), Inches(12.2), Inches(1.2), bg=CARD_BG, border=EMERALD)
add_text(s3, "Root Cause:", Inches(0.8), Inches(4.72), Inches(2), Inches(0.4),
         font_size=Pt(13), bold=True, color=EMERALD, font_name="Calibri")
add_text(s3,
         "There is NO platform in Pakistan that connects food-surplus restaurants with food-deficient "
         "charities in real time. Food gets wasted while millions go hungry — simply due to lack of coordination.",
         Inches(2.75), Inches(4.68), Inches(9.8), Inches(0.9),
         font_size=Pt(13), color=GRAY_400, font_name="Calibri", word_wrap=True)

add_rect(s3, 0, SLIDE_H - Inches(0.08), SLIDE_W, Inches(0.08), fill=RED_ACC)


# ═══════════════════════════════════════════════════════════════════
# SLIDE 4 — SOLUTION OVERVIEW
# ═══════════════════════════════════════════════════════════════════
s4 = add_slide()
add_rect(s4, 0, 0, Inches(0.18), SLIDE_H, fill=EMERALD)
gradient_header_bar(s4, EMERALD, EMERALD_DK)

add_text(s4, "02  ·  SOLUTION OVERVIEW", Inches(0.4), Inches(0.22),
         Inches(9), Inches(0.35), font_size=Pt(9), bold=True, color=EMERALD, font_name="Calibri")
add_text(s4, "ShareMeal Platform", Inches(0.55), Inches(0.7),
         Inches(9), Inches(0.6), font_size=Pt(32), bold=True, color=WHITE, font_name="Calibri")
add_rect(s4, Inches(0.55), Inches(1.32), Inches(2), Inches(0.05), fill=EMERALD)

portals = [
    ("🍽️", "Restaurant Portal",   "Donate Food",    "Post available food in one click. Manage donations, track status, view history.", EMERALD),
    ("🤲", "Charity Portal",      "Claim Food",     "Browse the food marketplace, claim donations, coordinate pickup with restaurant.", BLUE_ACC),
    ("👑", "Admin Panel",         "Manage All",     "Approve/reject organizations, monitor all activity, suspend accounts if needed.", PURPLE_ACC),
    ("🤖", "MealBot AI",          "AI Assistant",   "Google Gemini powered chatbot — answers in Roman Urdu & English 24/7.", GOLD_ACC),
]

for i, (icon, title, sub, desc, clr) in enumerate(portals):
    x = Inches(0.5) + (i % 2) * Inches(6.35)
    y = Inches(1.6)  + (i // 2) * Inches(2.4)
    card(s4, x, y, Inches(5.9), Inches(2.1), bg=CARD_BG, border=clr)
    add_text(s4, icon, x + Inches(0.18), y + Inches(0.22), Inches(0.9), Inches(0.9),
             font_size=Pt(36), color=clr, align=PP_ALIGN.CENTER, font_name="Segoe UI Emoji")
    add_text(s4, title, x + Inches(1.15), y + Inches(0.2), Inches(4.5), Inches(0.42),
             font_size=Pt(17), bold=True, color=WHITE, font_name="Calibri")
    add_text(s4, sub, x + Inches(1.15), y + Inches(0.62), Inches(4.5), Inches(0.32),
             font_size=Pt(11), bold=True, color=clr, font_name="Calibri")
    add_text(s4, desc, x + Inches(0.2), y + Inches(1.08), Inches(5.5), Inches(0.85),
             font_size=Pt(11), color=GRAY_400, font_name="Calibri", word_wrap=True)

add_rect(s4, 0, SLIDE_H - Inches(0.08), SLIDE_W, Inches(0.08), fill=EMERALD)


# ═══════════════════════════════════════════════════════════════════
# SLIDE 5 — SYSTEM ARCHITECTURE
# ═══════════════════════════════════════════════════════════════════
s5 = add_slide()
add_rect(s5, 0, 0, Inches(0.18), SLIDE_H, fill=PURPLE_ACC)
gradient_header_bar(s5, PURPLE_ACC, PURPLE_ACC)

add_text(s5, "03  ·  SYSTEM ARCHITECTURE", Inches(0.4), Inches(0.22),
         Inches(9), Inches(0.35), font_size=Pt(9), bold=True, color=PURPLE_ACC, font_name="Calibri")
add_text(s5, "MVC Architecture & Flow", Inches(0.55), Inches(0.7),
         Inches(9), Inches(0.6), font_size=Pt(32), bold=True, color=WHITE, font_name="Calibri")
add_rect(s5, Inches(0.55), Inches(1.32), Inches(2.5), Inches(0.05), fill=PURPLE_ACC)

# Flow boxes
flow = [
    ("👤", "User\n(Browser)", EMERALD),
    ("🎮", "Controller\n(Traffic Police)", BLUE_ACC),
    ("🗄️", "Database\n(SQLite)", PURPLE_ACC),
    ("🤖", "Gemini AI\n(Google)", GOLD_ACC),
    ("📄", "View (UI)\n(Razor + Tailwind)", EMERALD),
]
box_w = Inches(2.0)
box_h = Inches(1.5)
gap_x = Inches(0.45)
start_x = Inches(0.5)
flow_y = Inches(1.65)

for i, (icon, label, clr) in enumerate(flow):
    bx = start_x + i * (box_w + gap_x)
    card(s5, bx, flow_y, box_w, box_h, bg=CARD_BG, border=clr)
    add_text(s5, icon, bx, flow_y + Inches(0.1), box_w, Inches(0.7),
             font_size=Pt(30), color=clr, align=PP_ALIGN.CENTER, font_name="Segoe UI Emoji")
    add_text(s5, label, bx, flow_y + Inches(0.78), box_w, Inches(0.65),
             font_size=Pt(10), bold=True, color=WHITE, align=PP_ALIGN.CENTER, font_name="Calibri", word_wrap=True)
    # Arrow
    if i < len(flow) - 1:
        ax = bx + box_w + Inches(0.05)
        ay = flow_y + Inches(0.65)
        add_text(s5, "→", ax, ay, gap_x, Inches(0.4),
                 font_size=Pt(20), color=GRAY_600, align=PP_ALIGN.CENTER, font_name="Calibri")

# Pattern cards
patterns = [
    ("MVC Pattern",            "Model → View → Controller\nIndustry standard architecture",              EMERALD),
    ("Dependency Injection",   "Services loosely coupled\nEasy to swap or extend",                       BLUE_ACC),
    ("Repository Pattern",     "Data access abstracted via DbContext\nClean separation of concerns",      PURPLE_ACC),
    ("Interface Abstraction",  "IGeminiAiService interface\nAI service can be replaced without breaking", GOLD_ACC),
]
for i, (title, desc, clr) in enumerate(patterns):
    x = Inches(0.5) + i * Inches(3.15)
    card(s5, x, Inches(3.38), Inches(2.95), Inches(1.9), bg=CARD_BG, border=clr)
    add_rect(s5, x, Inches(3.38), Inches(0.06), Inches(1.9), fill=clr)
    add_text(s5, title, x + Inches(0.18), Inches(3.5), Inches(2.7), Inches(0.4),
             font_size=Pt(12), bold=True, color=clr, font_name="Calibri")
    add_text(s5, desc,  x + Inches(0.18), Inches(3.95), Inches(2.7), Inches(1.1),
             font_size=Pt(10), color=GRAY_400, font_name="Calibri", word_wrap=True)

add_rect(s5, 0, SLIDE_H - Inches(0.08), SLIDE_W, Inches(0.08), fill=PURPLE_ACC)


# ═══════════════════════════════════════════════════════════════════
# SLIDE 6 — KEY FEATURES
# ═══════════════════════════════════════════════════════════════════
s6 = add_slide()
add_rect(s6, 0, 0, Inches(0.18), SLIDE_H, fill=GOLD_ACC)
gradient_header_bar(s6, GOLD_ACC, GOLD_ACC)

add_text(s6, "04  ·  KEY FEATURES", Inches(0.4), Inches(0.22),
         Inches(9), Inches(0.35), font_size=Pt(9), bold=True, color=GOLD_ACC, font_name="Calibri")
add_text(s6, "8 Major Features", Inches(0.55), Inches(0.7),
         Inches(9), Inches(0.6), font_size=Pt(32), bold=True, color=WHITE, font_name="Calibri")
add_rect(s6, Inches(0.55), Inches(1.32), Inches(1.5), Inches(0.05), fill=GOLD_ACC)

features = [
    ("🔐", "Role-Based Authentication",   "Separate login for Restaurant, Charity & Admin with ASP.NET Identity", EMERALD),
    ("🛒", "Food Marketplace",            "Real-time listing of available donations. Charities browse & claim",    BLUE_ACC),
    ("🤖", "AI Chatbot (MealBot)",        "Google Gemini 2.0 Flash — bilingual chatbot (Urdu + English)",          PURPLE_ACC),
    ("🎙️", "Voice Input",                 "Web Speech API — speak your query, it converts to text",               GOLD_ACC),
    ("✅", "Admin Verification System",   "Admin approves/rejects organizations — trust & safety layer",           EMERALD),
    ("📊", "Live Dashboard Stats",        "Real-time count of donations, organizations & meals served",            BLUE_ACC),
    ("📱", "Fully Responsive UI",         "Works on mobile, tablet, desktop — Tailwind CSS framework",            PURPLE_ACC),
    ("🌍", "Live Internet Deployment",    "Hosted on Railway.app with HTTPS, Docker, CI/CD auto-deploy",          GOLD_ACC),
]

cols = 4
for i, (icon, title, desc, clr) in enumerate(features):
    row = i // cols
    col = i % cols
    x = Inches(0.45) + col * Inches(3.18)
    y = Inches(1.55) + row * Inches(2.5)
    card(s6, x, y, Inches(3.0), Inches(2.25), bg=CARD_BG, border=clr)
    add_text(s6, icon, x, y + Inches(0.12), Inches(3.0), Inches(0.7),
             font_size=Pt(28), color=clr, align=PP_ALIGN.CENTER, font_name="Segoe UI Emoji")
    add_text(s6, title, x + Inches(0.15), y + Inches(0.85), Inches(2.7), Inches(0.45),
             font_size=Pt(12), bold=True, color=WHITE, font_name="Calibri", word_wrap=True)
    add_text(s6, desc, x + Inches(0.15), y + Inches(1.32), Inches(2.7), Inches(0.8),
             font_size=Pt(9.5), color=GRAY_400, font_name="Calibri", word_wrap=True)

add_rect(s6, 0, SLIDE_H - Inches(0.08), SLIDE_W, Inches(0.08), fill=GOLD_ACC)


# ═══════════════════════════════════════════════════════════════════
# SLIDE 7 — TECHNOLOGY STACK
# ═══════════════════════════════════════════════════════════════════
s7 = add_slide()
add_rect(s7, 0, 0, Inches(0.18), SLIDE_H, fill=EMERALD)
gradient_header_bar(s7, EMERALD, EMERALD_DK)

add_text(s7, "05  ·  TECHNOLOGY STACK", Inches(0.4), Inches(0.22),
         Inches(9), Inches(0.35), font_size=Pt(9), bold=True, color=EMERALD, font_name="Calibri")
add_text(s7, "Technologies Used", Inches(0.55), Inches(0.7),
         Inches(9), Inches(0.6), font_size=Pt(32), bold=True, color=WHITE, font_name="Calibri")
add_rect(s7, Inches(0.55), Inches(1.32), Inches(2), Inches(0.05), fill=EMERALD)

tech_groups = [
    ("⚙️", "Backend",    EMERALD,    ["ASP.NET Core 8", "C# Language", "Entity Framework Core", "ASP.NET Identity", "Razor Pages"]),
    ("🎨", "Frontend",   BLUE_ACC,   ["Razor Views (.cshtml)", "Tailwind CSS v3", "Vanilla JavaScript", "Web Speech API", "Chart.js"]),
    ("🤖", "AI & APIs",  PURPLE_ACC, ["Google Gemini 2.0 Flash", "REST API (HttpClient)", "System Prompt Engineering", "Dependency Injection"]),
    ("🚀", "DevOps",     GOLD_ACC,   ["Docker + Dockerfile", "Railway.app Hosting", "GitHub + CI/CD", "HTTPS / SSL", "SQLite Database"]),
]

for i, (icon, title, clr, items) in enumerate(tech_groups):
    x = Inches(0.45) + i * Inches(3.18)
    card(s7, x, Inches(1.55), Inches(3.0), Inches(5.25), bg=CARD_BG, border=clr)
    # Header strip
    add_rect(s7, x, Inches(1.55), Inches(3.0), Inches(0.85), fill=clr)
    add_text(s7, icon + "  " + title, x, Inches(1.68), Inches(3.0), Inches(0.55),
             font_size=Pt(15), bold=True, color=DARK_BG, align=PP_ALIGN.CENTER, font_name="Calibri")
    for j, item in enumerate(items):
        iy = Inches(2.55) + j * Inches(0.52)
        add_rect(s7, x + Inches(0.2), iy, Inches(0.08), Inches(0.08),
                 fill=clr)   # bullet dot
        add_text(s7, item, x + Inches(0.42), iy - Inches(0.06), Inches(2.5), Inches(0.4),
                 font_size=Pt(11.5), color=GRAY_400, font_name="Calibri")

add_rect(s7, 0, SLIDE_H - Inches(0.08), SLIDE_W, Inches(0.08), fill=EMERALD)


# ═══════════════════════════════════════════════════════════════════
# SLIDE 8 — DATABASE DESIGN
# ═══════════════════════════════════════════════════════════════════
s8 = add_slide()
add_rect(s8, 0, 0, Inches(0.18), SLIDE_H, fill=BLUE_ACC)
gradient_header_bar(s8, BLUE_ACC, BLUE_ACC)

add_text(s8, "06  ·  DATABASE DESIGN", Inches(0.4), Inches(0.22),
         Inches(9), Inches(0.35), font_size=Pt(9), bold=True, color=BLUE_ACC, font_name="Calibri")
add_text(s8, "Models & Relationships", Inches(0.55), Inches(0.7),
         Inches(9), Inches(0.6), font_size=Pt(32), bold=True, color=WHITE, font_name="Calibri")
add_rect(s8, Inches(0.55), Inches(1.32), Inches(2.2), Inches(0.05), fill=BLUE_ACC)

tables = [
    ("🏢", "Organization", EMERALD, [
        ("🔑 PK", "Id",               "int — Auto increment"),
        ("📝",    "Name",             "string(100) — Required"),
        ("🏷️",    "Type",             "Enum: Restaurant / Charity / NGO"),
        ("📧",    "ContactEmail",     "string — Validated email"),
        ("📞",    "Phone",            "string — Phone number"),
        ("📍",    "Address",          "string — Location"),
        ("✅",    "Status",           "Enum: Pending / Verified / Suspended"),
        ("🔗",    "OwnerId",          "FK → ASP.NET Identity User"),
    ]),
    ("🍱", "Donation", BLUE_ACC, [
        ("🔑 PK", "Id",               "int — Auto increment"),
        ("🍕",    "FoodItem",         "string(100) — Food name"),
        ("📦",    "Quantity",         "string — Amount/servings"),
        ("🗓️",    "ExpiryDate",       "DateTime — When food expires"),
        ("🏷️",    "Category",         "Enum: Prepared / Bakery / Dairy..."),
        ("📊",    "Status",           "Enum: Available/Claimed/Collected"),
        ("🔗 FK", "DonorId",          "FK → Organization (Restaurant)"),
        ("🔗 FK", "RecipientId",      "FK → Organization (Charity)"),
    ]),
    ("👤", "ASP.NET Identity", PURPLE_ACC, [
        ("",      "AspNetUsers",      "Built-in user table"),
        ("",      "AspNetRoles",      "Restaurant, Charity, Admin roles"),
        ("",      "AspNetUserRoles",  "User-role mapping"),
        ("",      "Password Hashing", "BCrypt — secure storage"),
        ("",      "Cookie Auth",      "Session-based authentication"),
        ("",      "Claims",           "Role claims for authorization"),
    ]),
]

for i, (icon, name, clr, fields) in enumerate(tables):
    x = Inches(0.45) + i * Inches(4.18)
    card(s8, x, Inches(1.55), Inches(4.0), Inches(5.2), bg=CARD_BG, border=clr)
    add_rect(s8, x, Inches(1.55), Inches(4.0), Inches(0.75), fill=clr)
    add_text(s8, icon + "  " + name, x, Inches(1.68), Inches(4.0), Inches(0.5),
             font_size=Pt(14), bold=True, color=DARK_BG, align=PP_ALIGN.CENTER, font_name="Calibri")
    for j, (sym, col, desc) in enumerate(fields):
        fy = Inches(2.42) + j * Inches(0.42)
        if sym:
            add_text(s8, sym, x + Inches(0.15), fy, Inches(0.65), Inches(0.35),
                     font_size=Pt(9), color=clr, font_name="Segoe UI Emoji")
        add_text(s8, col, x + Inches(0.82), fy, Inches(1.4), Inches(0.35),
                 font_size=Pt(10), bold=True, color=WHITE, font_name="Calibri")
        add_text(s8, desc, x + Inches(0.82), fy + Inches(0.18), Inches(3.0), Inches(0.25),
                 font_size=Pt(8.5), color=GRAY_600, font_name="Calibri")

add_rect(s8, 0, SLIDE_H - Inches(0.08), SLIDE_W, Inches(0.08), fill=BLUE_ACC)


# ═══════════════════════════════════════════════════════════════════
# SLIDE 9 — AI INTEGRATION
# ═══════════════════════════════════════════════════════════════════
s9 = add_slide()
add_rect(s9, 0, 0, Inches(0.18), SLIDE_H, fill=PURPLE_ACC)
gradient_header_bar(s9, PURPLE_ACC, PURPLE_ACC)

add_text(s9, "07  ·  AI INTEGRATION", Inches(0.4), Inches(0.22),
         Inches(9), Inches(0.35), font_size=Pt(9), bold=True, color=PURPLE_ACC, font_name="Calibri")
add_text(s9, "MealBot — AI Food Assistant", Inches(0.55), Inches(0.7),
         Inches(9), Inches(0.6), font_size=Pt(32), bold=True, color=WHITE, font_name="Calibri")
add_rect(s9, Inches(0.55), Inches(1.32), Inches(2.8), Inches(0.05), fill=PURPLE_ACC)

ai_cards = [
    ("🧠", "AI Model",          "Google Gemini 2.0 Flash\nLatest & fastest model from Google DeepMind",  PURPLE_ACC),
    ("📝", "System Prompt",     "Custom instructions in GeminiAiService.cs\nContextualized for ShareMeal Pakistan", BLUE_ACC),
    ("🌐", "Bilingual Support", "Automatically detects Roman Urdu or English\nReplies in the same language", EMERALD),
    ("🎙️", "Voice Input",       "Web Speech API integration\nSpeak → Text → AI responds",                 GOLD_ACC),
    ("🔌", "Architecture",      "IGeminiAiService interface\nDependency Injection for loose coupling",     PURPLE_ACC),
    ("🔐", "Security",          "API key stored as Railway environment variable\nNever exposed in source code", RED_ACC),
]

for i, (icon, title, desc, clr) in enumerate(ai_cards):
    x = Inches(0.45) + (i % 3) * Inches(4.2)
    y = Inches(1.58)  + (i // 3) * Inches(2.35)
    card(s9, x, y, Inches(4.0), Inches(2.1), bg=CARD_BG, border=clr)
    add_text(s9, icon, x + Inches(0.18), y + Inches(0.2), Inches(0.8), Inches(0.75),
             font_size=Pt(32), color=clr, align=PP_ALIGN.CENTER, font_name="Segoe UI Emoji")
    add_text(s9, title, x + Inches(1.1), y + Inches(0.22), Inches(2.8), Inches(0.42),
             font_size=Pt(14), bold=True, color=WHITE, font_name="Calibri")
    add_text(s9, desc, x + Inches(0.2), y + Inches(0.88), Inches(3.7), Inches(1.05),
             font_size=Pt(10.5), color=GRAY_400, font_name="Calibri", word_wrap=True)

add_rect(s9, 0, SLIDE_H - Inches(0.08), SLIDE_W, Inches(0.08), fill=PURPLE_ACC)


# ═══════════════════════════════════════════════════════════════════
# SLIDE 10 — LIVE DEPLOYMENT
# ═══════════════════════════════════════════════════════════════════
s10 = add_slide()
add_rect(s10, 0, 0, Inches(0.18), SLIDE_H, fill=GOLD_ACC)
gradient_header_bar(s10, GOLD_ACC, GOLD_ACC)

add_text(s10, "08  ·  LIVE DEPLOYMENT", Inches(0.4), Inches(0.22),
         Inches(9), Inches(0.35), font_size=Pt(9), bold=True, color=GOLD_ACC, font_name="Calibri")
add_text(s10, "Production Deployment", Inches(0.55), Inches(0.7),
         Inches(9), Inches(0.6), font_size=Pt(32), bold=True, color=WHITE, font_name="Calibri")
add_rect(s10, Inches(0.55), Inches(1.32), Inches(2.2), Inches(0.05), fill=GOLD_ACC)

deploy_steps = [
    ("1", "🐳 Dockerized",         "Multi-stage Dockerfile created\nBuild → ASP.NET SDK → Runtime image", EMERALD),
    ("2", "📤 GitHub Push",         "Code pushed to GitHub repository\ngit add → commit → push", BLUE_ACC),
    ("3", "🚂 Railway Hosting",     "Railway.app free tier\n\$5/month credit — no card required", PURPLE_ACC),
    ("4", "🔄 Auto CI/CD",          "GitHub push triggers auto-deploy\nRailway rebuilds Docker image", GOLD_ACC),
]

for i, (step, title, desc, clr) in enumerate(deploy_steps):
    x = Inches(0.45) + i * Inches(3.18)
    card(s10, x, Inches(1.58), Inches(3.0), Inches(2.35), bg=CARD_BG, border=clr)
    # Step number circle
    add_rect(s10, x + Inches(0.18), Inches(1.74), Inches(0.45), Inches(0.45), fill=clr)
    add_text(s10, step, x + Inches(0.18), Inches(1.74), Inches(0.45), Inches(0.45),
             font_size=Pt(14), bold=True, color=DARK_BG, align=PP_ALIGN.CENTER, font_name="Calibri")
    add_text(s10, title, x + Inches(0.78), Inches(1.8), Inches(2.1), Inches(0.42),
             font_size=Pt(13), bold=True, color=WHITE, font_name="Calibri")
    add_text(s10, desc, x + Inches(0.2), Inches(2.35), Inches(2.72), Inches(1.1),
             font_size=Pt(10.5), color=GRAY_400, font_name="Calibri", word_wrap=True)

# Key achievements
achieve = [
    ("🔐", "HTTPS / SSL",           "Automatic secure certificate on Railway", EMERALD),
    ("🌍", "Global Access",         "Anyone worldwide can access the website",  BLUE_ACC),
    ("⚡", "Zero Downtime",         "Railway keeps service always online",      PURPLE_ACC),
    ("🔑", "Secure API Keys",       "Keys stored as environment variables",     GOLD_ACC),
]
card(s10, Inches(0.45), Inches(4.1), Inches(12.2), Inches(2.7), bg=CARD_BG)
add_text(s10, "Deployment Achievements", Inches(0.65), Inches(4.22),
         Inches(6), Inches(0.4), font_size=Pt(13), bold=True, color=WHITE, font_name="Calibri")

for i, (icon, title, desc, clr) in enumerate(achieve):
    x = Inches(0.65) + i * Inches(3.0)
    add_rect(s10, x, Inches(4.75), Inches(0.06), Inches(1.75), fill=clr)
    add_text(s10, icon + "  " + title, x + Inches(0.2), Inches(4.8), Inches(2.7), Inches(0.4),
             font_size=Pt(12), bold=True, color=clr, font_name="Calibri")
    add_text(s10, desc, x + Inches(0.2), Inches(5.25), Inches(2.7), Inches(0.5),
             font_size=Pt(10), color=GRAY_400, font_name="Calibri", word_wrap=True)

# URL box
card(s10, Inches(0.45), Inches(6.1), Inches(12.2), Inches(0.65), bg=RGBColor(0x05, 0x2E, 0x1E), border=EMERALD)
add_text(s10, "🌐  Live URL:  https://sharemeal-platform-production.up.railway.app",
         Inches(0.7), Inches(6.22), Inches(12.0), Inches(0.42),
         font_size=Pt(13), bold=True, color=EMERALD, font_name="Calibri")

add_rect(s10, 0, SLIDE_H - Inches(0.08), SLIDE_W, Inches(0.08), fill=GOLD_ACC)


# ═══════════════════════════════════════════════════════════════════
# SLIDE 11 — TEAM & SUPERVISOR
# ═══════════════════════════════════════════════════════════════════
s11 = add_slide()
add_rect(s11, 0, 0, Inches(0.18), SLIDE_H, fill=EMERALD)
gradient_header_bar(s11, EMERALD, EMERALD_DK)

add_text(s11, "09  ·  TEAM & SUPERVISOR", Inches(0.4), Inches(0.22),
         Inches(9), Inches(0.35), font_size=Pt(9), bold=True, color=EMERALD, font_name="Calibri")
add_text(s11, "Meet the People Behind ShareMeal", Inches(0.55), Inches(0.7),
         Inches(9), Inches(0.6), font_size=Pt(30), bold=True, color=WHITE, font_name="Calibri")
add_rect(s11, Inches(0.55), Inches(1.32), Inches(3.5), Inches(0.05), fill=EMERALD)

# Supervisor card — full width
card(s11, Inches(0.5), Inches(1.5), Inches(12.2), Inches(1.5), bg=CARD_BG, border=GOLD_ACC)
add_rect(s11, Inches(0.5), Inches(1.5), Inches(0.12), Inches(1.5), fill=GOLD_ACC)
add_text(s11, "👩‍🏫", Inches(0.75), Inches(1.62), Inches(0.85), Inches(0.85),
         font_size=Pt(36), color=GOLD_ACC, align=PP_ALIGN.CENTER, font_name="Segoe UI Emoji")
add_text(s11, "Prof. Kinat", Inches(1.72), Inches(1.62), Inches(5), Inches(0.48),
         font_size=Pt(20), bold=True, color=WHITE, font_name="Calibri")
add_text(s11, "Project Supervisor", Inches(1.72), Inches(2.1), Inches(4), Inches(0.35),
         font_size=Pt(12), bold=True, color=GOLD_ACC, font_name="Calibri")
add_text(s11, "University of Southern Punjab, Multan  ·  Computer Science Department",
         Inches(6.5), Inches(1.72), Inches(6), Inches(0.42),
         font_size=Pt(12), color=GRAY_400, font_name="Calibri")
add_text(s11, "FYP Supervisor — Guided and supervised the ShareMeal Platform Final Year Project",
         Inches(6.5), Inches(2.16), Inches(6), Inches(0.5),
         font_size=Pt(11), color=GRAY_600, font_name="Calibri", word_wrap=True)

# Developer cards
devs = [
    ("👨‍💻", "Muhammad Khulfan",  "Lead Developer & iOS Engineer",    EMERALD, [
        "🎓  BSCS — University of Southern Punjab, Multan",
        "📍  South Punjab, Pakistan",
        "💼  iOS Engineer & Full Stack .NET Developer",
        "🚀  Built ShareMeal from scratch: Architecture, AI, UI, Backend",
        "🤖  Integrated Google Gemini AI — MealBot chatbot",
        "🌍  Deployed on Railway.app with Docker & CI/CD",
    ]),
    ("💻", "Abdullah Khalid",    "Team Member & Developer",           PURPLE_ACC, [
        "🎓  BSCS — Computer Science",
        "🤝  Team Member — ShareMeal FYP Project",
        "💡  Contributed to development and project collaboration",
        "📚  University of Southern Punjab",
    ]),
]

for i, (icon, name, role, clr, bullets) in enumerate(devs):
    x = Inches(0.5) + i * Inches(6.4)
    w = Inches(6.1)
    card(s11, x, Inches(3.2), w, Inches(3.95), bg=CARD_BG, border=clr)
    add_rect(s11, x, Inches(3.2), w, Inches(0.85), fill=clr)
    add_text(s11, icon + "  " + name, x, Inches(3.32), w, Inches(0.5),
             font_size=Pt(17), bold=True, color=DARK_BG, align=PP_ALIGN.CENTER, font_name="Calibri")
    add_text(s11, role, x, Inches(3.84), w, Inches(0.3),
             font_size=Pt(10), bold=True, color=DARK_BG, align=PP_ALIGN.CENTER, font_name="Calibri")
    for j, b in enumerate(bullets):
        by = Inches(4.18) + j * Inches(0.52)
        add_text(s11, b, x + Inches(0.25), by, w - Inches(0.35), Inches(0.45),
                 font_size=Pt(11), color=GRAY_400, font_name="Calibri")

add_rect(s11, 0, SLIDE_H - Inches(0.08), SLIDE_W, Inches(0.08), fill=EMERALD)


# ═══════════════════════════════════════════════════════════════════
# SLIDE 12 — THANK YOU
# ═══════════════════════════════════════════════════════════════════
s12 = add_slide()
add_rect(s12, 0, 0, Inches(0.18), SLIDE_H, fill=EMERALD)

# Big soft background icon
add_text(s12, "🍱", Inches(3), Inches(0.5), Inches(7), Inches(6.5),
         font_size=Pt(250), color=RGBColor(0x05, 0x20, 0x12), align=PP_ALIGN.CENTER, font_name="Segoe UI Emoji")

# Top stripe
add_rect(s12, 0, 0, SLIDE_W, Inches(0.08), fill=EMERALD)
add_rect(s12, 0, SLIDE_H - Inches(0.08), SLIDE_W, Inches(0.08), fill=EMERALD)

add_text(s12, "Thank You", Inches(0.5), Inches(0.7), Inches(12.3), Inches(1.6),
         font_size=Pt(80), bold=True, color=WHITE, align=PP_ALIGN.CENTER, font_name="Calibri")

add_rect(s12, Inches(2.5), Inches(2.35), Inches(8.3), Inches(0.06), fill=EMERALD)

add_text(s12,
         "ShareMeal — Connecting food-surplus restaurants with food-deficient charities.\n"
         "Reducing food waste. Fighting hunger. Building a better Pakistan. 🇵🇰",
         Inches(0.8), Inches(2.55), Inches(11.7), Inches(1.0),
         font_size=Pt(15), italic=True, color=GRAY_400, align=PP_ALIGN.CENTER, font_name="Calibri", word_wrap=True)

# Info row
add_text(s12, "🎓  BSCS Final Year Project   |   University of Southern Punjab, Multan   |   2026",
         Inches(0.8), Inches(3.75), Inches(11.7), Inches(0.42),
         font_size=Pt(12), color=EMERALD, align=PP_ALIGN.CENTER, font_name="Calibri")
add_text(s12, "Supervisor: Prof. Kinat   |   Developers: Muhammad Khulfan & Abdullah Khalid",
         Inches(0.8), Inches(4.18), Inches(11.7), Inches(0.42),
         font_size=Pt(12), color=GRAY_600, align=PP_ALIGN.CENTER, font_name="Calibri")

# Links
add_rect(s12, Inches(1.5), Inches(4.85), Inches(4.2), Inches(0.65), fill=CARD_BG)
add_rect(s12, Inches(1.5), Inches(4.85), Inches(4.2), Inches(0.65), fill=None, line_color=EMERALD, line_w=Pt(1.2))
add_text(s12, "🌍  Live Website: sharemeal-platform-production.up.railway.app",
         Inches(1.7), Inches(4.95), Inches(4.0), Inches(0.42),
         font_size=Pt(10), color=EMERALD, font_name="Calibri")

add_rect(s12, Inches(6.35), Inches(4.85), Inches(4.2), Inches(0.65), fill=CARD_BG)
add_rect(s12, Inches(6.35), Inches(4.85), Inches(4.2), Inches(0.65), fill=None, line_color=BLUE_ACC, line_w=Pt(1.2))
add_text(s12, "💻  GitHub: github.com/Khulfan42/ShareMeal-Platform",
         Inches(6.55), Inches(4.95), Inches(4.0), Inches(0.42),
         font_size=Pt(10), color=BLUE_ACC, font_name="Calibri")

add_text(s12, "Questions & Discussion Welcome  ✨",
         Inches(0.8), Inches(5.8), Inches(11.7), Inches(0.5),
         font_size=Pt(18), bold=True, color=WHITE, align=PP_ALIGN.CENTER, font_name="Calibri")


# ═══════════════════════════════════════════════════════════════════
# SAVE
# ═══════════════════════════════════════════════════════════════════
os.makedirs(os.path.dirname(OUTPUT), exist_ok=True)
prs.save(OUTPUT)
print(f"✅  Presentation saved to:\n    {OUTPUT}")
print(f"    Slides: {len(prs.slides)}")
