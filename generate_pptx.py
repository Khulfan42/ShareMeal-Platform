"""
ShareMeal Platform — Professional Academic FYP Defense Presentation
Department of Computer Science • University of Southern Punjab, Multan
Presented by: Muhammad Khulfan & Abdullah Khalid
Supervised by: Prof. Kinat
"""

from pptx import Presentation
from pptx.util import Inches, Pt
from pptx.dml.color import RGBColor
from pptx.enum.text import PP_ALIGN
from pptx.oxml import parse_xml
import os

OUTPUT = os.path.join(os.path.dirname(__file__), "wwwroot", "ShareMeal_Presentation.pptx")
IMG_DIR = os.path.join(os.path.dirname(__file__), "wwwroot")

# ── Clean Executive Palette ───────────────────────────────────────
DARK_BG     = RGBColor(0x06, 0x09, 0x11)      # Deep obsidian
CARD_BG     = RGBColor(0x0F, 0x17, 0x2A)      # Slate card
EMERALD     = RGBColor(0x10, 0xB9, 0x81)      # Saffron/Life Green
AMBER       = RGBColor(0xF5, 0x9E, 0x0B)      # Supervisor Gold
ROSE        = RGBColor(0xF4, 0x3F, 0x5E)      # Hunger Crisis Rose
BLUE        = RGBColor(0x38, 0xBD, 0xF8)      # Tech Sky Blue
PURPLE      = RGBColor(0xC0, 0x84, 0xFC)      # AI Violet
WHITE       = RGBColor(0xFF, 0xFF, 0xFF)      # Crisp White
MUTED       = RGBColor(0x94, 0xA3, 0xB8)      # Slate Muted
BORDER      = RGBColor(0x1E, 0x29, 0x3B)      # Subtle Border

SLIDE_W = Inches(13.33)
SLIDE_H = Inches(7.5)

prs = Presentation()
prs.slide_width  = SLIDE_W
prs.slide_height = SLIDE_H

BLANK = prs.slide_layouts[6]

def add_rect(slide, x, y, w, h, fill=None, line_color=None, line_w=Pt(0)):
    shape = slide.shapes.add_shape(1, x, y, w, h)
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

def add_text(slide, text, x, y, w, h, font_size=Pt(14), bold=False, color=WHITE, align=PP_ALIGN.LEFT, font_name="Calibri"):
    tb = slide.shapes.add_textbox(x, y, w, h)
    tf = tb.text_frame
    tf.word_wrap = True
    p = tf.paragraphs[0]
    p.alignment = align
    run = p.add_run()
    run.text = text
    run.font.size = font_size
    run.font.bold = bold
    run.font.color.rgb = color
    run.font.name = font_name
    return tb

def add_slide(transition="fade"):
    slide = prs.slides.add_slide(BLANK)
    add_rect(slide, 0, 0, SLIDE_W, SLIDE_H, fill=DARK_BG)
    try:
        xml = parse_xml('<p:transition xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main" spd="med"><p:fade/></p:transition>')
        slide._element.append(xml)
    except:
        pass
    return slide

def slide_header(slide, section_tag, title, tag_color=EMERALD):
    add_rect(slide, Inches(0.8), Inches(0.55), Inches(0.08), Inches(0.75), fill=tag_color)
    add_text(slide, section_tag.upper(), Inches(1.05), Inches(0.5), Inches(8), Inches(0.3),
             font_size=Pt(10), bold=True, color=tag_color)
    add_text(slide, title, Inches(1.05), Inches(0.75), Inches(11), Inches(0.65),
             font_size=Pt(28), bold=True, color=WHITE)

def add_safe_pic(slide, filename, x, y, w, h):
    path = os.path.join(IMG_DIR, filename)
    if os.path.exists(path):
        try:
            return slide.shapes.add_picture(path, x, y, w, h)
        except:
            pass
    return None

# ═══════════════════════════════════════════════════════════
# SLIDE 1: COVER
# ═══════════════════════════════════════════════════════════
s1 = add_slide()

# University tag
add_rect(s1, Inches(0.8), Inches(0.8), Inches(7.5), Inches(0.4), fill=CARD_BG, line_color=EMERALD, line_w=Pt(1))
add_text(s1, "🏛️  DEPARTMENT OF COMPUTER SCIENCE  •  USP MULTAN", Inches(0.95), Inches(0.87), Inches(7.2), Inches(0.3),
         font_size=Pt(10), bold=True, color=EMERALD)

# Main Title
add_text(s1, "ShareMeal Platform", Inches(0.8), Inches(1.5), Inches(11.5), Inches(1.2),
         font_size=Pt(56), bold=True, color=WHITE)
add_text(s1, "A Web-Based Surplus Food Redistribution & Zero-Hunger Management System",
         Inches(0.8), Inches(2.7), Inches(11), Inches(0.5),
         font_size=Pt(18), bold=False, color=EMERALD)

# Divider line
add_rect(s1, Inches(0.8), Inches(3.4), Inches(11.7), Inches(0.02), fill=BORDER)

# Presented By Card
add_rect(s1, Inches(0.8), Inches(3.8), Inches(5.6), Inches(2.4), fill=CARD_BG, line_color=EMERALD, line_w=Pt(1))
add_text(s1, "PRESENTED BY", Inches(1.1), Inches(4.0), Inches(5.0), Inches(0.3),
         font_size=Pt(10), bold=True, color=EMERALD)
add_text(s1, "Muhammad Khulfan", Inches(1.1), Inches(4.35), Inches(5.0), Inches(0.4),
         font_size=Pt(18), bold=True, color=WHITE)
add_text(s1, "Abdullah Khalid", Inches(1.1), Inches(4.8), Inches(5.0), Inches(0.4),
         font_size=Pt(18), bold=True, color=WHITE)
add_text(s1, "BS Computer Science (BSCS) • Final Year Project", Inches(1.1), Inches(5.35), Inches(5.0), Inches(0.4),
         font_size=Pt(12), color=MUTED)

# Supervised By Card
add_rect(s1, Inches(6.9), Inches(3.8), Inches(5.6), Inches(2.4), fill=CARD_BG, line_color=AMBER, line_w=Pt(1))
add_text(s1, "SUPERVISED BY", Inches(7.2), Inches(4.0), Inches(5.0), Inches(0.3),
         font_size=Pt(10), bold=True, color=AMBER)
add_text(s1, "Prof. Kinat", Inches(7.2), Inches(4.35), Inches(5.0), Inches(0.4),
         font_size=Pt(22), bold=True, color=WHITE)
add_text(s1, "Department of Computer Science", Inches(7.2), Inches(4.95), Inches(5.0), Inches(0.35),
         font_size=Pt(13), color=MUTED)
add_text(s1, "University of Southern Punjab, Multan", Inches(7.2), Inches(5.35), Inches(5.0), Inches(0.35),
         font_size=Pt(13), color=AMBER)

add_text(s1, "Academic Session 2022–2026 • Final Project Defense", Inches(0.8), Inches(6.6), Inches(11.7), Inches(0.35),
         font_size=Pt(11), color=MUTED, align=PP_ALIGN.CENTER)

# ═══════════════════════════════════════════════════════════
# SLIDE 2: AGENDA
# ═══════════════════════════════════════════════════════════
s2 = add_slide()
slide_header(s2, "Overview", "Presentation Roadmap & Agenda")

agenda = [
    ("01", "The Crisis: Hunger & Survival", "Real food dilemma in Pakistan"),
    ("02", "How Food is Provided",          "From surplus kitchen to shelter plates"),
    ("03", "ShareMeal Solution",            "4 operational pillars bridging the gap"),
    ("04", "System Architecture",           "MVC design pattern, controllers & DB"),
    ("05", "Core User Roles",               "Restaurant, Charity, and Admin workflows"),
    ("06", "Technology Stack",              "ASP.NET Core 8, Tailwind, SQLite, Docker"),
    ("07", "MealBot Gemini AI",             "Roman Urdu NLP + Web Speech Voice API"),
    ("08", "Interactive Showcase",          "Production screens & cloud live URL"),
    ("09", "Social & Climate Impact",       "Plates diverted, carbon emission reduction"),
    ("10", "Future Scope & Expansion",      "Mobile apps, GPS van fleet & IoT sensors"),
]

for i, (num, title, desc) in enumerate(agenda):
    x = Inches(0.8) + (i % 2) * Inches(6.0)
    y = Inches(1.8) + (i // 2) * Inches(1.0)
    add_rect(s2, x, y, Inches(5.7), Inches(0.88), fill=CARD_BG, line_color=BORDER, line_w=Pt(1))
    add_rect(s2, x + Inches(0.2), y + Inches(0.18), Inches(0.55), Inches(0.55), fill=DARK_BG, line_color=EMERALD, line_w=Pt(1))
    add_text(s2, num, x + Inches(0.2), y + Inches(0.25), Inches(0.55), Inches(0.45), font_size=Pt(12), bold=True, color=EMERALD, align=PP_ALIGN.CENTER)
    add_text(s2, title, x + Inches(0.95), y + Inches(0.14), Inches(4.5), Inches(0.32), font_size=Pt(13), bold=True, color=WHITE)
    add_text(s2, desc, x + Inches(0.95), y + Inches(0.46), Inches(4.5), Inches(0.32), font_size=Pt(10), color=MUTED)

# ═══════════════════════════════════════════════════════════
# SLIDE 3: THE CRISIS: HUNGER & SURVIVAL IN PAKISTAN
# ═══════════════════════════════════════════════════════════
s3 = add_slide()
slide_header(s3, "Section 01 • The Problem", "Hunger & Survival in Pakistan", ROSE)

# Picture left
add_safe_pic(s3, "food_wastage.png", Inches(0.8), Inches(1.8), Inches(5.5), Inches(3.6))
add_rect(s3, Inches(0.8), Inches(5.5), Inches(5.5), Inches(0.8), fill=CARD_BG, line_color=ROSE, line_w=Pt(1))
add_text(s3, "Documentary Evidence: Over 36 Million Tons of edible food discarded annually in Pakistan.",
         Inches(1.0), Inches(5.6), Inches(5.1), Inches(0.6), font_size=Pt(11), color=WHITE)

# Right: Facts
cards = [
    ("Acute Hunger & Malnutrition", "Over 20+ million people face daily starvation. 40% of Pakistani children suffer from stunted physical and cognitive growth.", ROSE),
    ("Banquet & Restaurant Discard", "Banquet halls, hotels, and dine-in restaurants discard fresh, safe meals every night due to zero communication bridge.", AMBER),
    ("The Need for Digital Intervention", "Pakistan does not lack food — it lacks an instantaneous digital distribution network to rescue surplus before it spoils.", EMERALD),
]

for j, (t, d, c) in enumerate(cards):
    cy = Inches(1.8) + j * Inches(1.5)
    add_rect(s3, Inches(6.8), cy, Inches(5.7), Inches(1.35), fill=CARD_BG, line_color=c, line_w=Pt(1))
    add_text(s3, t, Inches(7.1), cy + Inches(0.15), Inches(5.1), Inches(0.35), font_size=Pt(14), bold=True, color=c)
    add_text(s3, d, Inches(7.1), cy + Inches(0.55), Inches(5.1), Inches(0.7), font_size=Pt(11), color=MUTED)

# ═══════════════════════════════════════════════════════════
# SLIDE 4: HOW FOOD IS PROVIDED
# ═══════════════════════════════════════════════════════════
s4 = add_slide()
slide_header(s4, "Section 02 • Distribution Flow", "How Food is Provided to the Needy", AMBER)

stages = [
    ("restaurant_bg.png",        "Stage 01: Surplus Logged",   "Restaurant enters food category, quantity & expiry timeline in 60s."),
    ("charity_hero_premium.png", "Stage 02: Real-Time Claim",  "Nearby registered charities receive alerts & claim meals instantly."),
    ("charity_volunteers.png",   "Stage 03: Hygiene Transport", "Volunteers collect sealed food containers adhering to safety SOPs."),
    ("food_serving.png",         "Stage 04: Dignified Serving", "Fresh food distributed at orphanages, slum clusters & hospitals."),
]

for i, (img, st_title, st_desc) in enumerate(stages):
    x = Inches(0.8) + i * Inches(2.98)
    add_safe_pic(s4, img, x, Inches(1.8), Inches(2.8), Inches(2.1))
    add_rect(s4, x, Inches(4.0), Inches(2.8), Inches(1.8), fill=CARD_BG, line_color=BORDER, line_w=Pt(1))
    add_text(s4, st_title, x + Inches(0.15), Inches(4.15), Inches(2.5), Inches(0.35), font_size=Pt(12), bold=True, color=WHITE)
    add_text(s4, st_desc, x + Inches(0.15), Inches(4.55), Inches(2.5), Inches(1.15), font_size=Pt(10), color=MUTED)

add_rect(s4, Inches(0.8), Inches(6.0), Inches(11.7), Inches(0.7), fill=CARD_BG, line_color=AMBER, line_w=Pt(1))
add_text(s4, "Target Beneficiaries:  Orphanages  •  Old Age Homes  •  Labor Points  •  Slum Settlements  •  Hospital Attendants",
         Inches(1.0), Inches(6.18), Inches(11.3), Inches(0.35), font_size=Pt(11.5), bold=True, color=WHITE, align=PP_ALIGN.CENTER)

# ═══════════════════════════════════════════════════════════
# SLIDE 5: SHAREMEAL SOLUTION
# ═══════════════════════════════════════════════════════════
s5 = add_slide()
slide_header(s5, "Section 03 • Proposed System", "The ShareMeal Ecosystem")

sols = [
    ("1", "Dedicated Role-Based Portals", "Custom tailored interfaces for Restaurants to donate, Charities to claim, and System Admin to govern."),
    ("2", "Live Food Marketplace",        "Real-time listing catalog filtered by expiry deadline, quantity, and city location."),
    ("3", "Admin Trust & Hygiene Gate",   "Only verified NGOs and licensed food outlets can participate, ensuring zero health hazards."),
    ("4", "AI Relief Guidance (MealBot)", "Google Gemini 2.0 assistant answers questions in natural Roman Urdu and English with voice support."),
]

for i, (step, title, desc) in enumerate(sols):
    x = Inches(0.8) + (i % 2) * Inches(6.0)
    y = Inches(1.8) + (i // 2) * Inches(2.4)
    add_rect(s5, x, y, Inches(5.7), Inches(2.1), fill=CARD_BG, line_color=EMERALD, line_w=Pt(1))
    add_rect(s5, x + Inches(0.3), y + Inches(0.25), Inches(0.55), Inches(0.55), fill=DARK_BG, line_color=EMERALD, line_w=Pt(1))
    add_text(s5, step, x + Inches(0.3), y + Inches(0.32), Inches(0.55), Inches(0.45), font_size=Pt(14), bold=True, color=EMERALD, align=PP_ALIGN.CENTER)
    add_text(s5, title, x + Inches(1.05), y + Inches(0.25), Inches(4.3), Inches(0.4), font_size=Pt(15), bold=True, color=WHITE)
    add_text(s5, desc, x + Inches(0.3), y + Inches(0.95), Inches(5.1), Inches(1.0), font_size=Pt(11.5), color=MUTED)

# ═══════════════════════════════════════════════════════════
# SLIDE 6: SYSTEM ARCHITECTURE
# ═══════════════════════════════════════════════════════════
s6 = add_slide()
slide_header(s6, "Section 04 • Technical Design", "System Architecture (3-Tier MVC Pattern)", BLUE)

tiers = [
    ("🖥️", "Presentation Layer", "Razor Views (.cshtml)", "Tailwind CSS responsive design, JavaScript voice input, dynamic interactive forms."),
    ("⚙️", "Application Layer", "ASP.NET Core Controllers", "Handles authentication, donation workflows, admin claims, and Gemini AI service."),
    ("🗄️", "Data Storage Layer", "SQLite & Entity Framework", "Structured relational database storing users, verified organizations, and active donations."),
]

for i, (icon, tier, tech, desc) in enumerate(tiers):
    x = Inches(0.8) + i * Inches(4.0)
    add_rect(s6, x, Inches(1.8), Inches(3.7), Inches(3.6), fill=CARD_BG, line_color=BLUE, line_w=Pt(1))
    add_text(s6, icon, x, Inches(2.1), Inches(3.7), Inches(0.7), font_size=Pt(36), align=PP_ALIGN.CENTER)
    add_text(s6, tier, x + Inches(0.2), Inches(2.9), Inches(3.3), Inches(0.4), font_size=Pt(16), bold=True, color=WHITE, align=PP_ALIGN.CENTER)
    add_text(s6, tech, x + Inches(0.2), Inches(3.35), Inches(3.3), Inches(0.35), font_size=Pt(12), bold=True, color=BLUE, align=PP_ALIGN.CENTER)
    add_text(s6, desc, x + Inches(0.3), Inches(3.8), Inches(3.1), Inches(1.3), font_size=Pt(11), color=MUTED, align=PP_ALIGN.CENTER)

add_rect(s6, Inches(0.8), Inches(5.8), Inches(11.7), Inches(0.9), fill=CARD_BG, line_color=BORDER, line_w=Pt(1))
add_text(s6, "Pipeline: Browser Client ➔ Controller Action ➔ Model Validation ➔ EF Core DbContext ➔ Rendered HTML View",
         Inches(1.0), Inches(6.05), Inches(11.3), Inches(0.4), font_size=Pt(11.5), bold=True, color=MUTED, align=PP_ALIGN.CENTER)

# ═══════════════════════════════════════════════════════════
# SLIDE 7: CORE USER WORKFLOWS
# ═══════════════════════════════════════════════════════════
s7 = add_slide()
slide_header(s7, "Section 05 • User Roles", "User Roles & Operational Workflows")

roles = [
    ("🍽️ Restaurant Donor", [
        "Create dedicated restaurant profile",
        "Post available meals with expiration time",
        "Set special pickup notes & contact details",
        "Mark donations collected upon handover",
    ]),
    ("🤲 Verified Charity", [
        "Register charity or orphanage profile",
        "Browse real-time marketplace of food",
        "Claim available meals in one click",
        "Coordinate pickup directly with restaurant",
    ]),
    ("👑 System Admin", [
        "Review pending organization submissions",
        "Approve or reject donor / charity accounts",
        "Audit food safety compliance",
        "Monitor live national donation metrics",
    ]),
]

for i, (title, bullets) in enumerate(roles):
    x = Inches(0.8) + i * Inches(4.0)
    add_rect(s7, x, Inches(1.8), Inches(3.7), Inches(4.9), fill=CARD_BG, line_color=BORDER, line_w=Pt(1))
    add_text(s7, title, x + Inches(0.3), Inches(2.1), Inches(3.1), Inches(0.5), font_size=Pt(16), bold=True, color=WHITE)
    add_rect(s7, x + Inches(0.3), Inches(2.65), Inches(1.5), Inches(0.04), fill=EMERALD)
    for j, b in enumerate(bullets):
        by = Inches(2.9) + j * Inches(0.85)
        add_text(s7, "✓  " + b, x + Inches(0.3), by, Inches(3.1), Inches(0.75), font_size=Pt(11.5), color=MUTED)

# ═══════════════════════════════════════════════════════════
# SLIDE 8: TECH STACK
# ═══════════════════════════════════════════════════════════
s8 = add_slide()
slide_header(s8, "Section 06 • Implementation", "Technology Stack & Tooling", AMBER)

techs = [
    ("C# & ASP.NET Core 8", "Enterprise-grade server framework handling security, dependency injection, and high throughput."),
    ("Tailwind CSS Framework", "Utility-first modern styling ensuring 100% responsiveness on mobile, tablet, and desktop."),
    ("SQLite & EF Core", "Lightweight, reliable relational database with automated schema migrations and ACID transactions."),
    ("Google Gemini 2.0 Flash", "State-of-the-art conversational AI providing instant bilingual responses in Roman Urdu & English."),
    ("Docker Linux Containers", "Self-contained application image packaging code, database, and dependencies for zero-friction setup."),
    ("Railway.app Cloud Hosting", "Production cloud deployment with automated GitHub CI/CD, SSL/TLS encryption, and continuous uptime."),
]

for i, (title, desc) in enumerate(techs):
    x = Inches(0.8) + (i % 2) * Inches(6.0)
    y = Inches(1.8) + (i // 2) * Inches(1.6)
    add_rect(s8, x, y, Inches(5.7), Inches(1.35), fill=CARD_BG, line_color=BORDER, line_w=Pt(1))
    add_text(s8, title, x + Inches(0.3), y + Inches(0.18), Inches(5.1), Inches(0.35), font_size=Pt(14), bold=True, color=WHITE)
    add_text(s8, desc, x + Inches(0.3), y + Inches(0.55), Inches(5.1), Inches(0.7), font_size=Pt(10.5), color=MUTED)

# ═══════════════════════════════════════════════════════════
# SLIDE 9: AI ASSISTANT (MEALBOT)
# ═══════════════════════════════════════════════════════════
s9 = add_slide()
slide_header(s9, "Section 07 • Artificial Intelligence", "MealBot: AI Relief Assistant", PURPLE)

add_rect(s9, Inches(0.8), Inches(1.8), Inches(5.7), Inches(4.8), fill=CARD_BG, line_color=PURPLE, line_w=Pt(1))
add_text(s9, "Key AI Capabilities", Inches(1.1), Inches(2.1), Inches(5.1), Inches(0.4), font_size=Pt(16), bold=True, color=WHITE)

ai_caps = [
    ("Bilingual NLP (Roman Urdu)", "Understands Pakistani user queries written in Roman Urdu as well as standard English."),
    ("Web Speech API Voice Input", "Microphone feature converts spoken Urdu/English speech into text directly inside the browser."),
    ("Domain Context Prompting", "Instructed with full knowledge of ShareMeal operations, Pakistani cities, and food safety standards."),
    ("Zero Infrastructure Cost", "Uses Google Gemini free tier API key injected securely via cloud environment variables."),
]

for j, (cap_title, cap_desc) in enumerate(ai_caps):
    cy = Inches(2.65) + j * Inches(0.95)
    add_text(s9, "• " + cap_title, Inches(1.1), cy, Inches(5.1), Inches(0.3), font_size=Pt(12), bold=True, color=PURPLE)
    add_text(s9, cap_desc, Inches(1.3), cy + Inches(0.28), Inches(4.9), Inches(0.55), font_size=Pt(10.5), color=MUTED)

# Right: Chat Mockup
add_rect(s9, Inches(6.9), Inches(1.8), Inches(5.6), Inches(4.8), fill=CARD_BG, line_color=BORDER, line_w=Pt(1))
add_text(s9, "🤖  Live Interaction Sample", Inches(7.2), Inches(2.1), Inches(5.0), Inches(0.4), font_size=Pt(14), bold=True, color=WHITE)

add_rect(s9, Inches(7.2), Inches(2.7), Inches(5.0), Inches(0.9), fill=DARK_BG, line_color=BORDER, line_w=Pt(1))
add_text(s9, "User: 'Multan mein kis jagah se khana claim kar sakte hain?'", Inches(7.4), Inches(2.95), Inches(4.6), Inches(0.4), font_size=Pt(11), color=MUTED)

add_rect(s9, Inches(7.2), Inches(3.8), Inches(5.0), Inches(1.7), fill=DARK_BG, line_color=EMERALD, line_w=Pt(1))
add_text(s9, "MealBot AI:\n'Asslam o Alikum! ShareMeal Marketplace par login karein — Multan ke verified restaurants ke posted donations available hain. Charity portal se direct claim karein!'",
         Inches(7.4), Inches(3.95), Inches(4.6), Inches(1.35), font_size=Pt(11), color=EMERALD)

# ═══════════════════════════════════════════════════════════
# SLIDE 10: INTERACTIVE SHOWCASE SLIDER
# ═══════════════════════════════════════════════════════════
s10 = add_slide()
slide_header(s10, "Section 08 • System Demonstration", "Platform Interface Showcase", BLUE)

demos = [
    ("carousel_1.png", "Home & Zero-Waste", "Landing page highlighting mission, stats & responsive navigation."),
    ("carousel_2.png", "Community Relief",  "Card-based food catalog displaying available donations for charities."),
    ("carousel_3.png", "Fresh Produce Hub", "Surplus donation form with category, expiry deadline & notes."),
    ("admin_bg.png",   "Admin Governance",  "Central control panel for approving/rejecting organizations."),
]

for i, (img, title, desc) in enumerate(demos):
    x = Inches(0.8) + i * Inches(2.98)
    add_safe_pic(s10, img, x, Inches(1.8), Inches(2.8), Inches(2.2))
    add_rect(s10, x, Inches(4.1), Inches(2.8), Inches(1.5), fill=CARD_BG, line_color=BORDER, line_w=Pt(1))
    add_text(s10, title, x + Inches(0.15), Inches(4.25), Inches(2.5), Inches(0.3), font_size=Pt(12), bold=True, color=WHITE)
    add_text(s10, desc, x + Inches(0.15), Inches(4.6), Inches(2.5), Inches(0.9), font_size=Pt(10), color=MUTED)

add_rect(s10, Inches(0.8), Inches(5.9), Inches(11.7), Inches(0.8), fill=CARD_BG, line_color=EMERALD, line_w=Pt(1))
add_text(s10, "🌐  Production Cloud URL:  https://sharemeal-platform-production.up.railway.app",
         Inches(1.0), Inches(6.12), Inches(11.3), Inches(0.4), font_size=Pt(13), bold=True, color=EMERALD, align=PP_ALIGN.CENTER)

# ═══════════════════════════════════════════════════════════
# SLIDE 11: SOCIAL IMPACT & RESULTS
# ═══════════════════════════════════════════════════════════
s11 = add_slide()
slide_header(s11, "Section 09 • Real Results", "Measurable Social & Environmental Impact", EMERALD)

impacts = [
    ("500+", "Meals Rescued", "Fresh plates diverted from garbage bins"),
    ("50+", "Partner Organizations", "Verified restaurants and shelter homes"),
    ("850 kg", "CO₂ Emissions Averted", "Reduced decomposing landfill methane"),
    ("100%", "Zero Commission", "Free open humanitarian platform"),
]

for i, (num, title, sub) in enumerate(impacts):
    x = Inches(0.8) + i * Inches(2.98)
    add_rect(s11, x, Inches(1.8), Inches(2.8), Inches(3.0), fill=CARD_BG, line_color=EMERALD, line_w=Pt(1))
    add_text(s11, num, x + Inches(0.2), Inches(2.4), Inches(2.4), Inches(0.8), font_size=Pt(36), bold=True, color=EMERALD, align=PP_ALIGN.CENTER)
    add_text(s11, title, x + Inches(0.2), Inches(3.3), Inches(2.4), Inches(0.4), font_size=Pt(14), bold=True, color=WHITE, align=PP_ALIGN.CENTER)
    add_text(s11, sub, x + Inches(0.2), Inches(3.75), Inches(2.4), Inches(0.7), font_size=Pt(11), color=MUTED, align=PP_ALIGN.CENTER)

add_rect(s11, Inches(0.8), Inches(5.2), Inches(11.7), Inches(1.3), fill=CARD_BG, line_color=BORDER, line_w=Pt(1))
add_text(s11, "Social Dignity Principle: ShareMeal treats food distribution not as charity handouts, but as dignified sustenance — food is cataloged with strict expiry hours, hygienically sealed, and handed over with mutual respect.",
         Inches(1.1), Inches(5.45), Inches(11.1), Inches(0.8), font_size=Pt(12), color=WHITE, align=PP_ALIGN.CENTER)

# ═══════════════════════════════════════════════════════════
# SLIDE 12: FUTURE SCOPE & ROADMAP
# ═══════════════════════════════════════════════════════════
s12 = add_slide()
slide_header(s12, "Section 10 • Future Roadmap", "Project Scope & Nationwide Expansion", AMBER)

scopes = [
    ("📱 Native Mobile Apps (iOS & Android)", "Dedicated mobile application with instant push notifications when a nearby restaurant posts fresh surplus food."),
    ("📍 Live GPS Van Fleet Tracking", "Integration of Google Maps Platform for real-time tracking of charity volunteer pickup vans and ETA calculation."),
    ("🌡️ IoT Smart Temperature Sensors", "Automated freshness & thermal sensors to verify food was stored above 60°C or below 5°C before distribution."),
    ("🇵🇰 Multi-City Expansion", "Scaling beyond Multan to Lahore, Karachi, Rawalpindi, and Islamabad with localized NGO hubs."),
]

for i, (title, desc) in enumerate(scopes):
    x = Inches(0.8) + (i % 2) * Inches(6.0)
    y = Inches(1.8) + (i // 2) * Inches(2.3)
    add_rect(s12, x, y, Inches(5.7), Inches(2.0), fill=CARD_BG, line_color=AMBER, line_w=Pt(1))
    add_text(s12, title, x + Inches(0.3), y + Inches(0.25), Inches(5.1), Inches(0.4), font_size=Pt(14), bold=True, color=WHITE)
    add_text(s12, desc, x + Inches(0.3), y + Inches(0.8), Inches(5.1), Inches(0.95), font_size=Pt(11.5), color=MUTED)

# ═══════════════════════════════════════════════════════════
# SLIDE 13: SUPERVISION & CONCLUSION
# ═══════════════════════════════════════════════════════════
s13 = add_slide()

# Supervisor Card
add_rect(s13, Inches(0.8), Inches(0.8), Inches(11.7), Inches(1.6), fill=CARD_BG, line_color=AMBER, line_w=Pt(1))
add_text(s13, "👩‍🏫", Inches(1.1), Inches(1.1), Inches(0.8), Inches(0.8), font_size=Pt(36), align=PP_ALIGN.CENTER)
add_text(s13, "PROJECT SUPERVISOR", Inches(2.1), Inches(1.05), Inches(5), Inches(0.3), font_size=Pt(10), bold=True, color=AMBER)
add_text(s13, "Prof. Kinat", Inches(2.1), Inches(1.35), Inches(5), Inches(0.45), font_size=Pt(20), bold=True, color=WHITE)
add_text(s13, "Department of Computer Science • University of Southern Punjab, Multan", Inches(2.1), Inches(1.85), Inches(8), Inches(0.35), font_size=Pt(12), color=MUTED)

# Dev 1 (with real picture)
add_rect(s13, Inches(0.8), Inches(2.7), Inches(5.7), Inches(3.2), fill=CARD_BG, line_color=EMERALD, line_w=Pt(1))
add_safe_pic(s13, "dev_pic.png", Inches(1.1), Inches(2.95), Inches(1.0), Inches(1.0))
add_text(s13, "LEAD DEVELOPER & ARCHITECT", Inches(2.3), Inches(2.95), Inches(4.0), Inches(0.3), font_size=Pt(10), bold=True, color=EMERALD)
add_text(s13, "Muhammad Khulfan", Inches(2.3), Inches(3.25), Inches(4.0), Inches(0.4), font_size=Pt(18), bold=True, color=WHITE)
add_text(s13, "BSCS • iOS Engineer & Full Stack .NET", Inches(2.3), Inches(3.7), Inches(4.0), Inches(0.35), font_size=Pt(12), bold=True, color=BLUE)
add_text(s13, "Engineered platform architecture, database schemas, Tailwind frontend, Gemini AI integration, voice recording, and cloud containerization.",
         Inches(1.1), Inches(4.2), Inches(5.1), Inches(1.4), font_size=Pt(11), color=MUTED)

# Dev 2
add_rect(s13, Inches(6.8), Inches(2.7), Inches(5.7), Inches(3.2), fill=CARD_BG, line_color=BORDER, line_w=Pt(1))
add_rect(s13, Inches(7.1), Inches(2.95), Inches(1.0), Inches(1.0), fill=DARK_BG, line_color=BORDER, line_w=Pt(1))
add_text(s13, "AK", Inches(7.1), Inches(3.2), Inches(1.0), Inches(0.5), font_size=Pt(20), bold=True, color=WHITE, align=PP_ALIGN.CENTER)
add_text(s13, "TEAM MEMBER & DEVELOPER", Inches(8.3), Inches(2.95), Inches(4.0), Inches(0.3), font_size=Pt(10), bold=True, color=MUTED)
add_text(s13, "Abdullah Khalid", Inches(8.3), Inches(3.25), Inches(4.0), Inches(0.4), font_size=Pt(18), bold=True, color=WHITE)
add_text(s13, "BS Computer Science (BSCS)", Inches(8.3), Inches(3.7), Inches(4.0), Inches(0.35), font_size=Pt(12), bold=True, color=WHITE)
add_text(s13, "Collaborated on system requirements, feature testing, project documentation, and verification throughout the FYP lifecycle.",
         Inches(7.1), Inches(4.2), Inches(5.1), Inches(1.4), font_size=Pt(11), color=MUTED)

add_text(s13, "Thank You! Questions & External Viva Discussion Welcome  ✨", Inches(0.8), Inches(6.3), Inches(11.7), Inches(0.5),
         font_size=Pt(18), bold=True, color=WHITE, align=PP_ALIGN.CENTER)

# Save
prs.save(OUTPUT)
print(f"✅ Masterwork Academic Presentation saved to {OUTPUT} (13 slides with images)")
