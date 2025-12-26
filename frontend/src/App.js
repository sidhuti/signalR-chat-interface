import { useState, useRef, useEffect } from 'react';
import { useSignalR } from './useSignalR';
import './App.css';

function App() {
  const hubUrl = 'http://localhost:5000/chathub';
  const { isConnected, connectionId, chatHistory, isProcessing, sendPrompt, stopGeneration, clearHistory } = useSignalR(hubUrl);
  
  const [input, setInput] = useState('');
  const messagesEndRef = useRef(null);

  // Auto-scroll to bottom when new messages arrive
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [chatHistory]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (input.trim() && !isProcessing) {
      await sendPrompt(input);
      setInput('');
    }
  };

  const handleKeyDown = (e) => {
    // Send on Enter, new line on Shift+Enter
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSubmit(e);
    }
  };

  return (
    <div className="app">
      {/* Header */}
      <header className="header">
        <div className="header-content">
          <h1>AI Chat</h1>
          <div className="header-actions">
            <div className="connection-status">
              <span className={`status-dot ${isConnected ? 'connected' : 'disconnected'}`}></span>
              <span className="status-text">
                {isConnected ? 'Connected' : 'Disconnected'}
              </span>
            </div>
            {chatHistory.length > 0 && (
              <button onClick={clearHistory} className="btn-clear" title="Clear chat">
                Clear
              </button>
            )}
          </div>
        </div>
      </header>

      {/* Chat Messages */}
      <div className="chat-container">
        <div className="messages">
          {chatHistory.length === 0 ? (
            <div className="welcome">
              <h2>Welcome to AI Chat</h2>
              <p>Ask me anything! Responses arrive in ~10 seconds.</p>
              <div className="suggestions">
                <button onClick={() => setInput('Explain quantum computing in simple terms')} className="suggestion">
                  Explain quantum computing
                </button>
                <button onClick={() => setInput('Write a haiku about programming')} className="suggestion">
                  Write a haiku
                </button>
                <button onClick={() => setInput('What are the benefits of TypeScript?')} className="suggestion">
                  TypeScript benefits
                </button>
              </div>
            </div>
          ) : (
            <>
              {chatHistory.map((message, index) => (
                <div key={index} className={`message ${message.role}`}>
                  <div className="message-header">
                    <span className="role-badge">
                      {message.role === 'user' ? '👤 You' : message.role === 'assistant' ? '🤖 Assistant' : '⚠️ Error'}
                    </span>
                    <span className="timestamp">
                      {message.timestamp.toLocaleTimeString()}
                    </span>
                  </div>
                  <div className="message-content">
                    {message.content}
                  </div>
                </div>
              ))}
              
              {/* Processing indicator */}
              {isProcessing && (
                <div className="message assistant processing">
                  <div className="message-header">
                    <span className="role-badge">🤖 Assistant</span>
                  </div>
                  <div className="message-content">
                    <div className="typing-indicator">
                      <span></span>
                      <span></span>
                      <span></span>
                    </div>
                    <span className="processing-text">Thinking...</span>
                  </div>
                </div>
              )}
            </>
          )}
          <div ref={messagesEndRef} />
        </div>
      </div>

      {/* Input Area */}
      <div className="input-container">
        <form onSubmit={handleSubmit} className="input-form">
          <textarea
            value={input}
            onChange={(e) => setInput(e.target.value)}
            onKeyDown={handleKeyDown}
            placeholder="Type your message... (Press Enter to send, Shift+Enter for new line)"
            className="input-textarea"
            disabled={!isConnected}
            rows={1}
          />
          <div className="input-actions">
            {isProcessing ? (
              <button
                type="button"
                onClick={stopGeneration}
                className="btn-stop"
              >
                ⬛ Stop
              </button>
            ) : (
              <button
                type="submit"
                disabled={!isConnected || !input.trim() || isProcessing}
                className="btn-send"
              >
                ➤ Send
              </button>
            )}
          </div>
        </form>
        <div className="input-footer">
          <span className="hint">
            {isProcessing ? 'Generating response...' : 'Enter to send • Shift+Enter for new line'}
          </span>
        </div>
      </div>
    </div>
  );
}

export default App;
