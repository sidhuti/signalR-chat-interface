import { useEffect, useState, useCallback } from 'react';
import * as signalR from '@microsoft/signalr';

export const useSignalR = (hubUrl) => {
  const [connection, setConnection] = useState(null);
  const [isConnected, setIsConnected] = useState(false);
  const [connectionId, setConnectionId] = useState(null);
  const [chatHistory, setChatHistory] = useState([]);
  const [isProcessing, setIsProcessing] = useState(false);

  // Create connection
  useEffect(() => {
    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl)
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    setConnection(newConnection);
  }, [hubUrl]);

  // Start connection and set up listeners
  useEffect(() => {
    if (connection) {
      connection
        .start()
        .then(() => {
          console.log('Connected to SignalR hub');
          setIsConnected(true);
        })
        .catch((error) => console.error('Connection failed: ', error));

      // When connection is established
      connection.on('Connected', (connId) => {
        console.log('Connection ID:', connId);
        setConnectionId(connId);
      });

      // When prompt is acknowledged
      connection.on('PromptReceived', (prompt) => {
        console.log('Prompt received:', prompt);
        setIsProcessing(true);
        
        // Add user message to chat history
        setChatHistory((prev) => [
          ...prev,
          {
            role: 'user',
            content: prompt,
            timestamp: new Date(),
          },
        ]);
      });

      // When response arrives
      connection.on('ReceiveResponse', (prompt, response) => {
        console.log('Response received');
        setIsProcessing(false);
        
        // Add assistant response to chat history
        setChatHistory((prev) => [
          ...prev,
          {
            role: 'assistant',
            content: response,
            timestamp: new Date(),
          },
        ]);
      });

      // When generation is stopped
      connection.on('GenerationStopped', () => {
        console.log('Generation stopped');
        setIsProcessing(false);
      });

      // When an error occurs
      connection.on('Error', (errorMessage) => {
        console.error('Error from server:', errorMessage);
        setIsProcessing(false);
        
        // Add error message to chat
        setChatHistory((prev) => [
          ...prev,
          {
            role: 'error',
            content: errorMessage,
            timestamp: new Date(),
          },
        ]);
      });

      // Handle reconnection
      connection.onreconnecting(() => {
        setIsConnected(false);
        console.log('Reconnecting...');
      });

      connection.onreconnected(() => {
        setIsConnected(true);
        setConnectionId(connection.connectionId || null);
        console.log('Reconnected');
      });

      connection.onclose(() => {
        setIsConnected(false);
        console.log('Connection closed');
      });

      // Cleanup on unmount
      return () => {
        connection.stop();
      };
    }
  }, [connection]);

  // Send a prompt (no sessionId needed)
  const sendPrompt = useCallback(
    async (prompt) => {
      if (connection && isConnected && !isProcessing) {
        try {
          await connection.invoke('SendPrompt', prompt);
        } catch (error) {
          console.error('Error sending prompt: ', error);
        }
      }
    },
    [connection, isConnected, isProcessing]
  );

  // Stop current generation
  const stopGeneration = useCallback(
    async () => {
      if (connection && isConnected && isProcessing) {
        try {
          await connection.invoke('StopGeneration');
        } catch (error) {
          console.error('Error stopping generation: ', error);
        }
      }
    },
    [connection, isConnected, isProcessing]
  );

  // Clear chat history
  const clearHistory = useCallback(() => {
    setChatHistory([]);
  }, []);

  return {
    isConnected,
    connectionId,
    chatHistory,
    isProcessing,
    sendPrompt,
    stopGeneration,
    clearHistory,
  };
};
