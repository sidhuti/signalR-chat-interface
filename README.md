# SignalR Chat Interface (ChatGPT-Style)

A real-time chat application with a **single-session conversational interface** like ChatGPT, built with SignalR (C# backend) and React (frontend).

## 🎯 Design Philosophy

**Single Session Per Tab:**
- Each browser tab = one ongoing conversation
- User waits for response before sending next message
- Chat history displays like ChatGPT
- Clean, focused UX

**vs. Multi-Session (previous design):**
- ❌ Multiple parallel requests
- ❌ Complex session cards
- ❌ Overwhelming UI

## ✨ Features

- ✅ **ChatGPT-style interface** - Clean, modern chat UI
- ✅ **Real-time responses** - No polling, instant delivery
- ✅ **Chat history** - All messages preserved in conversation
- ✅ **Typing indicator** - Shows when AI is "thinking"
- ✅ **Stop generation** - Cancel mid-response
- ✅ **Auto-scroll** - Always shows latest message
- ✅ **Dark theme** - Easy on the eyes
- ✅ **Keyboard shortcuts** - Enter to send, Shift+Enter for newline
- ✅ **Suggestion chips** - Quick start prompts
- ✅ **No timeouts** - WebSocket stays open indefinitely

## 🏗️ Architecture

### Simplified Design

```
User Tab (Browser)
    ↓
One SignalR Connection (ConnectionId)
    ↓
One Active Session at a Time
    ↓
Chat History (Array of messages)
```

### Key Simplifications

| Aspect | Old (Multi-Session) | New (Chat) |
|--------|---------------------|------------|
| **Session ID** | Generated UUID | Uses ConnectionId |
| **UI** | Multiple cards | Chat bubbles |
| **State** | Dictionary of sessions | Array of messages |
| **User Flow** | Send many, track all | Send one, wait, repeat |


## Running the Application

### Backend

```bash
cd backend
dotnet restore
dotnet build
dotnet run
```

Runs on: `https://localhost:5000`

### Frontend

```bash
cd frontend
npm install
npm install --save-dev ajv@^7 
npm start
```

Runs on: `http://localhost:3000`

## 💡 How to Use

1. **Start both servers**
2. **Open** `http://localhost:3000` in browser
3. **Type a message** in the input box
4. **Press Enter** to send (or click Send button)
5. **See "Thinking..."** indicator
6. **Wait ~10 seconds** (simulating LLM processing)
7. **Response appears** in chat!
8. **Continue conversation** - each exchange builds on previous
