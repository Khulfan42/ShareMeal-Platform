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

# ── Clean Executive Palette ───────────────────────────────────────
DARK_BG     = RGBColor(0x09, 0x0D, 0x16)      # Deep executive slate
CARD_BG     = RGBColor(0x11, 0x18, 0x27)      # Clean card
EMERALD     = RGBColor(0x10, 0xB9, 0x81)      # Accent green
AMBER       = RGBColor(0xF5, 0x9E, 0x0B)      # Supervisor gold
BLUE        = RGBColor(0x3B, 0x82, 0xF6)      # Technology blue
WHITE       = RGBColor(0xFF, 0xFF, 0xFF)      # High contrast text
MUTED       = RGBColor(0x94, 0xA3, 0xB8)      # Secondary text
BORDER      = RGBColor(0x1E, 0x29, 0x3B)      # Subtle border

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

def slide_header(slide, section_tag, title):
    add_rect(slide, Inches(0.8), Inches(0.55), Inches(0.08), Inches(0.75), fill=EMERALD)
    add_text(slide, section_tag.upper(), Inches(1.05), Inches(0.5), Inches(8), Inches(0.3),
             font_size=Pt(10), bold=True, color=EMERALD)
    add_text(slide, title, Inches(1.05), Inches(0.75), Inches(11), Inches(0.65),
             font_size=Pt(28), bold=True, color=WHITE)

# ═══════════════════════════════════════════════════════════════════
# SLIDE 1: COVER
# ═══════════════════════════════════════════════════════════════════
s1 = add_slide()

# University banner tag
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

# Bottom credit
add_text(s1, "Academic Session 2022–2026 • Final Project Defense", Inches(0.8), Inches(6.6), Inches(11.7), Inches(0.35),
         font_size=Pt(11), color=MUTED, align=PP_ALIGN.CENTER)

# ═══════════════════════════════════════════════════════════════════
# SLIDE 2: AGENDA
# ═══════════════════════════════════════════════════════════════════
s2 = add_slide()
slide_header(s2, "Overview", "Presentation Roadmap & Agenda")

agenda = [
    ("01", "Problem Statement",       "Food wastage & hunger dilemma in Pakistan"),
    ("02", "Proposed Solution",       "ShareMeal core workflow and vision"),
    ("03", "System Architecture",     "MVC design pattern, controllers & data flow"),
    ("04", "Core User Portals",       "Restaurant, Charity, and Admin interactions"),
    ("05", "Technology Stack",        "ASP.NET Core 8, C#, Tailwind CSS & SQLite"),
    ("06", "AI Assistant (MealBot)",  "Google Gemini 2.0 Flash + Web Speech Voice"),
    ("07", "Live Demonstration",      "Interface walk-through & production URL"),
    ("08", "Project Supervision",     "Supervisor and development team credits"),
]

for i, (num, title, desc) in enumerate(agenda):
    x = Inches(0.8) + (i % 2) * Inches(6.0)
    y = Inches(1.8) + (i // 2) * Inches(1.2)
    add_rect(s2, x, y, Inches(5.7), Inches(1.0), fill=CARD_BG, line_color=BORDER, line_w=Pt(1))
    add_rect(s2, x + Inches(0.2), y + Inches(0.2), Inches(0.6), Inches(0.6), fill=DARK_BG, line_color=EMERALD, line_w=Pt(1))
    add_text(s2, num, x + Inches(0.2), y + Inches(0.28), Inches(0.6), Inches(0.5), font_size=Pt(13), bold=True, color=EMERALD, align=PP_ALIGN.CENTER)
    add_text(s2, title, x + Inches(1.0), y + Inches(0.18), Inches(4.5), Inches(0.35), font_size=Pt(14), bold=True, color=WHITE)
    add_text(s2, desc, x + Inches(1.0), y + Inches(0.52), Inches(4.5), Inches(0.35), font_size=Pt(10.5), color=MUTED)

# ═══════════════════════════════════════════════════════════
# SLIDE 3: PROBLEM STATEMENT
# ═══════════════════════════════════════════════════════════
s3 = add_slide()
slide_header(s3, "Section 01", "The Problem in Pakistan: Food Waste vs. Hunger")

probs = [
    ("🗑️", "36% Food Wasted", "Commercial eateries & banquet halls discard 36% of edible food daily due to absence of timely channels."),
    ("💔", "20M+ Face Hunger", "Over 20 million citizens and shelter home residents suffer from acute hunger and nutritional deficit."),
    ("🔌", "No Digital Bridge", "Restaurants have surplus food but lack logistics; charities need food but have zero real-time notification."),
]

for i, (icon, title, desc) in enumerate(probs):
    x = Inches(0.8) + i * Inches(4.0)
    add_rect(s3, x, Inches(1.8), Inches(3.7), Inches(3.6), fill=CARD_BG, line_color=BORDER, line_w=Pt(1))
    add_text(s3, icon, x, Inches(2.2), Inches(3.7), Inches(0.8), font_size=Pt(36), align=PP_ALIGN.CENTER)
    add_text(s3, title, x + Inches(0.2), Inches(3.1), Inches(3.3), Inches(0.45), font_size=Pt(16), bold=True, color=WHITE, align=PP_ALIGN.CENTER)
    add_text(s3, desc, x + Inches(0.3), Inches(3.65), Inches(3.1), Inches(1.5), font_size=Pt(11.5), color=MUTED, align=PP_ALIGN.CENTER)

# Bottom core takeaway
add_rect(s3, Inches(0.8), Inches(5.8), Inches(11.7), Inches(0.9), fill=CARD_BG, line_color=EMERALD, line_w=Pt(1))
add_text(s3, "Core Objective: Build a reliable, fast, zero-cost digital bridge between food donors and registered charities in Pakistan.",
         Inches(1.0), Inches(6.05), Inches(11.3), Inches(0.4), font_size=Pt(13), bold=True, color=WHITE, align=PP_ALIGN.CENTER)

# ═══════════════════════════════════════════════════════════
# SLIDE 4: PROPOSED SOLUTION
# ═══════════════════════════════════════════════════════════
s4 = add_slide()
slide_header(s4, "Section 02", "Proposed Solution: How ShareMeal Operates")

steps = [
    ("1", "Restaurants Post Meals", "Eateries enter food category, number of servings, expiry deadline, and pickup instructions in under 1 minute."),
    ("2", "Real-Time Marketplace", "Charities instantly view available donations on a live dashboard and claim meals before they spoil."),
    ("3", "Admin Trust Verification", "System Admin verifies legal documents of restaurants and NGOs to ensure complete hygiene and food safety."),
    ("4", "MealBot AI Assistance", "Google Gemini AI guides users in natural Roman Urdu and English on donations, claims, and pickup SOPs."),
]

for i, (step, title, desc) in enumerate(steps):
    x = Inches(0.8) + (i % 2) * Inches(6.0)
    y = Inches(1.8) + (i // 2) * Inches(2.4)
    add_rect(s4, x, y, Inches(5.7), Inches(2.1), fill=CARD_BG, line_color=EMERALD, line_w=Pt(1))
    add_rect(s4, x + Inches(0.3), y + Inches(0.25), Inches(0.55), Inches(0.55), fill=DARK_BG, line_color=EMERALD, line_w=Pt(1))
    add_text(s4, step, x + Inches(0.3), y + Inches(0.32), Inches(0.55), Inches(0.45), font_size=Pt(14), bold=True, color=EMERALD, align=PP_ALIGN.CENTER)
    add_text(s4, title, x + Inches(1.05), y + Inches(0.25), Inches(4.3), Inches(0.4), font_size=Pt(15), bold=True, color=WHITE)
    add_text(s4, desc, x + Inches(0.3), y + Inches(0.95), Inches(5.1), Inches(1.0), font_size=Pt(11.5), color=MUTED)

# ═══════════════════════════════════════════════════════════
# SLIDE 5: SYSTEM ARCHITECTURE
# ═══════════════════════════════════════════════════════════
s5 = add_slide()
slide_header(s5, "Section 03", "System Architecture: 3-Tier MVC Pattern")

tiers = [
    ("🖥️", "Presentation Tier", "Razor Views (.cshtml)", "Tailwind CSS responsive UI, JavaScript voice input, dynamic interactive forms."),
    ("⚙️", "Application Tier", "ASP.NET Core Controllers", "Handles authentication, donation workflows, admin claims, and Gemini AI service."),
    ("🗄️", "Data Tier", "SQLite & Entity Framework", "Structured relational database storing users, verified organizations, and active donations."),
]

for i, (icon, tier, tech, desc) in enumerate(tiers):
    x = Inches(0.8) + i * Inches(4.0)
    add_rect(s5, x, Inches(1.8), Inches(3.7), Inches(3.6), fill=CARD_BG, line_color=BLUE, line_w=Pt(1))
    add_text(s5, icon, x, Inches(2.1), Inches(3.7), Inches(0.7), font_size=Pt(36), align=PP_ALIGN.CENTER)
    add_text(s5, tier, x + Inches(0.2), Inches(2.9), Inches(3.3), Inches(0.4), font_size=Pt(16), bold=True, color=WHITE, align=PP_ALIGN.CENTER)
    add_text(s5, tech, x + Inches(0.2), Inches(3.35), Inches(3.3), Inches(0.35), font_size=Pt(12), bold=True, color=BLUE, align=PP_ALIGN.CENTER)
    add_text(s5, desc, x + Inches(0.3), Inches(3.8), Inches(3.1), Inches(1.3), font_size=Pt(11), color=MUTED, align=PP_ALIGN.CENTER)

add_rect(s5, Inches(0.8), Inches(5.8), Inches(11.7), Inches(0.9), fill=CARD_BG, line_color=BORDER, line_w=Pt(1))
add_text(s5, "Request Flow: Client Browser ➔ HTTPS Request ➔ Controller Routing ➔ Business Logic ➔ EF Core SQLite ➔ Rendered HTML",
         Inches(1.0), Inches(6.05), Inches(11.3), Inches(0.4), font_size=Pt(11.5), bold=True, color=MUTED, align=PP_ALIGN.CENTER)

# ═══════════════════════════════════════════════════════════
# SLIDE 6: CORE USER WORKFLOWS
# ═══════════════════════════════════════════════════════════
s6 = add_slide()
slide_header(s6, "Section 04", "User Roles & Operational Workflows")

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
    add_rect(s6, x, Inches(1.8), Inches(3.7), Inches(4.9), fill=CARD_BG, line_color=BORDER, line_w=Pt(1))
    add_text(s6, title, x + Inches(0.3), Inches(2.1), Inches(3.1), Inches(0.5), font_size=Pt(16), bold=True, color=WHITE)
    add_rect(s6, x + Inches(0.3), Inches(2.65), Inches(1.5), Inches(0.04), fill=EMERALD)
    for j, b in enumerate(bullets):
        by = Inches(2.9) + j * Inches(0.85)
        add_text(s6, "✓  " + b, x + Inches(0.3), by, Inches(3.1), Inches(0.75), font_size=Pt(11.5), color=MUTED)

# ═══════════════════════════════════════════════════════════
# SLIDE 7: TECH STACK
# ═══════════════════════════════════════════════════════════
s7 = add_slide()
slide_header(s7, "Section 05", "Technology Stack & Tooling")

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
    add_rect(s7, x, y, Inches(5.7), Inches(1.35), fill=CARD_BG, line_color=BORDER, line_w=Pt(1))
    add_text(s7, title, x + Inches(0.3), y + Inches(0.18), Inches(5.1), Inches(0.35), font_size=Pt(14), bold=True, color=WHITE)
    add_text(s7, desc, x + Inches(0.3), y + Inches(0.55), Inches(5.1), Inches(0.7), font_size=Pt(10.5), color=MUTED)

# ═══════════════════════════════════════════════════════════
# SLIDE 8: AI ASSISTANT (MEALBOT)
# ═══════════════════════════════════════════════════════════
s8 = add_slide()
slide_header(s8, "Section 06", "MealBot: AI-Powered Relief Assistant")

add_rect(s8, Inches(0.8), Inches(1.8), Inches(5.7), Inches(4.8), fill=CARD_BG, line_color=EMERALD, line_w=Pt(1))
add_text(s8, "Key AI Capabilities", Inches(1.1), Inches(2.1), Inches(5.1), Inches(0.4), font_size=Pt(16), bold=True, color=WHITE)

ai_caps = [
    ("Bilingual NLP (Roman Urdu)", "Understands Pakistani user queries written in Roman Urdu as well as standard English."),
    ("Web Speech API Voice Input", "Microphone feature converts spoken Urdu/English speech into text directly inside the browser."),
    ("Domain Context Prompting", "Instructed with full knowledge of ShareMeal operations, Pakistani cities, and food safety standards."),
    ("Zero Infrastructure Cost", "Uses Google Gemini free tier API key injected securely via cloud environment variables."),
]

for j, (cap_title, cap_desc) in enumerate(ai_caps):
    cy = Inches(2.65) + j * Inches(0.95)
    add_text(s8, "• " + cap_title, Inches(1.1), cy, Inches(5.1), Inches(0.3), font_size=Pt(12), bold=True, color=EMERALD)
    add_text(s8, cap_desc, Inches(1.3), cy + Inches(0.28), Inches(4.9), Inches(0.55), font_size=Pt(10.5), color=MUTED)

# Right: Chat Mockup
add_rect(s8, Inches(6.9), Inches(1.8), Inches(5.6), Inches(4.8), fill=CARD_BG, line_color=BORDER, line_w=Pt(1))
add_text(s8, "🤖  Live Interaction Sample", Inches(7.2), Inches(2.1), Inches(5.0), Inches(0.4), font_size=Pt(14), bold=True, color=WHITE)

add_rect(s8, Inches(7.2), Inches(2.7), Inches(5.0), Inches(0.9), fill=DARK_BG, line_color=BORDER, line_w=Pt(1))
add_text(s8, "User: 'Multan mein khana kahan available hai?'", Inches(7.4), Inches(2.95), Inches(4.6), Inches(0.4), font_size=Pt(11), color=MUTED)

add_rect(s8, Inches(7.2), Inches(3.8), Inches(5.0), Inches(1.7), fill=DARK_BG, line_color=EMERALD, line_w=Pt(1))
add_text(s8, "MealBot AI:\n'Asslam o Alikum! ShareMeal Marketplace par login karein — Multan ke verified restaurants ke posted donations available hain. Charity portal se direct claim karein!'",
         Inches(7.4), Inches(3.95), Inches(4.6), Inches(1.35), font_size=Pt(11), color=EMERALD)

# ═══════════════════════════════════════════════════════════
# SLIDE 9: LIVE DEMO & SLIDER
# ═══════════════════════════════════════════════════════════
s9 = add_slide()
slide_header(s9, "Section 07", "Live Demonstration & System Interfaces")

demos = [
    ("🏠 Home Page", "Landing page highlighting zero food waste mission, live statistics counter, and responsive navigation."),
    ("🍽️ Restaurant Panel", "Donation post form with category dropdown, expiry deadline, pickup notes, and active donation history."),
    ("🤲 Charity Marketplace", "Card-based food catalog displaying available donations with one-click claim buttons."),
    ("👑 Admin Dashboard", "Centralized control panel for approving/rejecting organizations and auditing platform health."),
]

for i, (title, desc) in enumerate(demos):
    x = Inches(0.8) + (i % 2) * Inches(6.0)
    y = Inches(1.8) + (i // 2) * Inches(1.9)
    add_rect(s9, x, y, Inches(5.7), Inches(1.6), fill=CARD_BG, line_color=BORDER, line_w=Pt(1))
    add_text(s9, title, x + Inches(0.3), y + Inches(0.2), Inches(5.1), Inches(0.35), font_size=Pt(15), bold=True, color=WHITE)
    add_text(s9, desc, x + Inches(0.3), y + Inches(0.6), Inches(5.1), Inches(0.85), font_size=Pt(11), color=MUTED)

add_rect(s9, Inches(0.8), Inches(5.9), Inches(11.7), Inches(0.8), fill=CARD_BG, line_color=EMERALD, line_w=Pt(1))
add_text(s9, "🌐  Production Cloud URL:  https://sharemeal-platform-production.up.railway.app",
         Inches(1.0), Inches(6.12), Inches(11.3), Inches(0.4), font_size=Pt(13), bold=True, color=EMERALD, align=PP_ALIGN.CENTER)

# ═══════════════════════════════════════════════════════════
# SLIDE 10: ACADEMIC SUPERVISION & DEVELOPERS
# ═══════════════════════════════════════════════════════════
s10 = add_slide()
slide_header(s10, "Academic Credits", "Project Supervision & Engineering Team")

# Supervisor Card
add_rect(s10, Inches(0.8), Inches(1.8), Inches(11.7), Inches(1.6), fill=CARD_BG, line_color=AMBER, line_w=Pt(1))
add_text(s10, "👩‍🏫", Inches(1.1), Inches(2.1), Inches(0.8), Inches(0.8), font_size=Pt(36), align=PP_ALIGN.CENTER)
add_text(s10, "ACADEMIC SUPERVISOR", Inches(2.1), Inches(2.05), Inches(5), Inches(0.3), font_size=Pt(10), bold=True, color=AMBER)
add_text(s10, "Prof. Kinat", Inches(2.1), Inches(2.35), Inches(5), Inches(0.45), font_size=Pt(20), bold=True, color=WHITE)
add_text(s10, "Department of Computer Science • University of Southern Punjab, Multan", Inches(2.1), Inches(2.85), Inches(8), Inches(0.35), font_size=Pt(12), color=MUTED)

# Developer 1: Muhammad Khulfan
add_rect(s10, Inches(0.8), Inches(3.7), Inches(5.7), Inches(3.0), fill=CARD_BG, line_color=EMERALD, line_w=Pt(1))
add_text(s10, "LEAD DEVELOPER & ARCHITECT", Inches(1.1), Inches(3.95), Inches(5.1), Inches(0.3), font_size=Pt(10), bold=True, color=EMERALD)
add_text(s10, "Muhammad Khulfan", Inches(1.1), Inches(4.25), Inches(5.1), Inches(0.4), font_size=Pt(18), bold=True, color=WHITE)
add_text(s10, "BS Computer Science (BSCS) • iOS Engineer", Inches(1.1), Inches(4.7), Inches(5.1), Inches(0.35), font_size=Pt(12), bold=True, color=BLUE)
add_text(s10, "Engineered platform architecture, database schemas, Tailwind frontend, Gemini AI integration, voice recording, and cloud containerization.",
         Inches(1.1), Inches(5.15), Inches(5.1), Inches(1.2), font_size=Pt(11), color=MUTED)

# Developer 2: Abdullah Khalid
add_rect(s10, Inches(6.8), Inches(3.7), Inches(5.7), Inches(3.0), fill=CARD_BG, line_color=BORDER, line_w=Pt(1))
add_text(s10, "TEAM MEMBER & DEVELOPER", Inches(7.1), Inches(3.95), Inches(5.1), Inches(0.3), font_size=Pt(10), bold=True, color=MUTED)
add_text(s10, "Abdullah Khalid", Inches(7.1), Inches(4.25), Inches(5.1), Inches(0.4), font_size=Pt(18), bold=True, color=WHITE)
add_text(s10, "BS Computer Science (BSCS)", Inches(7.1), Inches(4.7), Inches(5.1), Inches(0.35), font_size=Pt(12), bold=True, color=WHITE)
add_text(s10, "Collaborated on system requirements, feature testing, project documentation, and verification throughout the FYP lifecycle.",
         Inches(7.1), Inches(5.15), Inches(5.1), Inches(1.2), font_size=Pt(11), color=MUTED)

# ═══════════════════════════════════════════════════════════
# SLIDE 11: CONCLUSION & THANK YOU
# ═══════════════════════════════════════════════════════════
s11 = add_slide()

add_text(s11, "🍱", Inches(0.8), Inches(1.5), Inches(11.7), Inches(0.8), font_size=Pt(48), align=PP_ALIGN.CENTER)
add_text(s11, "Thank You", Inches(0.8), Inches(2.4), Inches(11.7), Inches(1.1), font_size=Pt(52), bold=True, color=WHITE, align=PP_ALIGN.CENTER)
add_text(s11, "ShareMeal Platform — Turning Food Waste into Dignified Meals for Pakistan",
         Inches(0.8), Inches(3.4), Inches(11.7), Inches(0.5), font_size=Pt(16), bold=True, color=EMERALD, align=PP_ALIGN.CENTER)

add_rect(s11, Inches(2.8), Inches(4.2), Inches(7.7), Inches(1.3), fill=CARD_BG, line_color=BORDER, line_w=Pt(1))
add_text(s11, "Special Gratitude to Supervisor: Prof. Kinat", Inches(3.0), Inches(4.4), Inches(7.3), Inches(0.35),
         font_size=Pt(13), bold=True, color=AMBER, align=PP_ALIGN.CENTER)
add_text(s11, "Department of Computer Science • University of Southern Punjab, Multan", Inches(3.0), Inches(4.8), Inches(7.3), Inches(0.35),
         font_size=Pt(12), color=MUTED, align=PP_ALIGN.CENTER)

add_text(s11, "Questions & External Viva Discussion Welcome  ✨", Inches(0.8), Inches(5.9), Inches(11.7), Inches(0.5),
         font_size=Pt(18), bold=True, color=WHITE, align=PP_ALIGN.CENTER)

# Save
prs.save(OUTPUT)
print(f"✅ Clean Academic Presentation saved to {OUTPUT} (11 slides)")
