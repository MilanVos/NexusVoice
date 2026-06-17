# NexusVoice 🇳🇱 ↔ 🇩🇪

Real time stem vertaler voor Nederlands ↔ Duits — speciaal ontworpen voor gesprekken via Discord of andere voip-apps.

Je spreekt Nederlands, de ander hoort Duits. De ander spreekt Duits, jij ziet de Nederlandse vertaling op je scherm.

![NexusVoice Screenshot](https://i.imgur.com/placeholder.png)

---

## ✨ Features

- 🎙 **Real-time spraakherkenning** — geen knoppen, gewoon praten
- 🔄 **Beide richtingen tegelijk** — NL→DE en DE→NL simultaan
- 🔊 **TTS via virtuele kabel** — werkt naadloos met Discord, TeamSpeak, etc.
- 📡 **Loopback capture** — herkent automatisch wat de ander zegt
- 💬 **Gespreklog** — scrollende chatgeschiedenis per sessie
- 🎙 **Stemkeuze** — meerdere mannelijke en vrouwelijke Azure neural voices
- 🌙 **Donker thema** — volledige dark UI



## 🚀 Setup

### Stap 1 — Download & installeer VB-Cable

1. Ga naar [vb-audio.com/Cable](https://vb-audio.com/Cable/)
2. Download `VBCABLE_Driver_Pack43.zip`
3. Pak uit → rechtsklik `VBCABLE_Setup_x64.exe` → **Als administrator uitvoeren**
4. Herstart je PC

---

### Stap 2 — Azure Speech API-sleutel aanmaken

1. Ga naar [portal.azure.com](https://portal.azure.com) en maak een gratis account aan
2. Klik op **Create a resource** → zoek op `Speech`
3. Maak een **Speech service** resource aan:
   - Pricing tier: **Free F0** (5 uur spraak/maand gratis)
   - Kies een regio dichtbij (bijv. `germanywestcentral` of `westeurope`)
4. Ga naar de resource → **Keys and Endpoint**
5. Kopieer **Key 1** en de **Location/Region** (bijv. `germanywestcentral`)

---

1. Open Discord → **Gebruikersinstellingen** (tandwiel links onder)
2. Ga naar **Spraak & Video**
3. Stel in:
   - **Invoerapparaat (Microfoon):** `CABLE Output (VB-Audio Virtual Cable)`
   - **Uitvoerapparaat (Speaker):** Jouw headset of speakers

> ⚠️ Let op: Invoer = `CABLE Output`, uitvoer = jouw echte headset



Start de app → klik op **⚙ Instellingen** en vul in:

| Veld | Wat invullen |
|---|---|
| **Azure API-sleutel** | De Key 1 van je Speech resource |
| **Regio** | Bijv. `germanywestcentral` of `westeurope` |
| **🎙 Microfoon** | Jouw headset microfoon |
| **🔊 Duits TTS uitvoer** | `CABLE Input (VB-Audio Virtual Cable)` |
| **📡 Discord uitvoer (loopback)** | Jouw headset / speakers (waar Discord uit speelt) |
| **Duitse stem** | Kies een mannelijke of vrouwelijke stem naar keuze |

Klik **💾 Opslaan**.

---

## 🎮 Gebruik

1. Start NexusVoice
2. Klik op **🎙 Start Vertaling**
3. Spreek Nederlands — de app vertaalt automatisch naar Duits en stuurt het via VB-Cable naar Discord
4. De ander spreekt Duits — jij ziet de Nederlandse vertaling live op je scherm

Klik **⏹ Stop Vertaling** om de sessie te beëindigen.

---

## 🔧 Hoe het werkt

```
Jij (NL mic) ──► Azure STT ──► Azure Translator ──► Azure TTS (DE)
                                                          │
                                                    CABLE Input
                                                          │
                                                       Discord ──► Duitser hoort Duits

Duitser (Discord) ──► Jouw headset ──► Loopback capture ──► Azure STT ──► Azure Translator
                                                                                    │
                                                                          NL tekst op scherm
```

