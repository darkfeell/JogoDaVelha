using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class VelhaGameClient
{
    private TcpClient client;
    private NetworkStream stream;
    private string serverIp = "192.168.88.254";
    private int serverPort = 13000;
    private char PlayerIcon; 
    private bool IsTime = false; 

    public VelhaGameClient()
    {
        client = new TcpClient();
        client.Connect(serverIp, serverPort);
        stream = client.GetStream();
        Console.WriteLine("Conectado ao servidor.");

        Thread receiveThread = new Thread(RecMessages);
        receiveThread.Start();
    }

    private void RecMessages()
    {
        byte[] buffer = new byte[2048];

        while (true)
        {
            try
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                ProcMessage(message); // Processa a mensagem recebida sem exibi-la
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao receber mensagem: {ex.Message}");
                break;
            }
        }

        client.Close();
    }

    private void ProcMessage(string message)
    {
        if (message == "1")
        {
            Console.WriteLine("Aguardando outro jogador...");
        }
        else if (message == "0")
        {
            Console.WriteLine("Partida começando!");
            if (PlayerIcon == 'X') // 'X' joga primeiro
            {
                IsTime = true;
                ReqMove();
            }
            else
            {
                Console.WriteLine("Aguarde o jogador X jogar.");
            }
        }
        else if (message.Length == 9) // Estado do tabuleiro
        {
            DisplayBoard(message);

            // Verifica se o jogo terminou com vitória ou empate
            if (message.EndsWith("1") || message.EndsWith("2") || message.EndsWith("3"))
            {
                IsTime = false; // Jogo terminou
            }
            // Se for a vez do jogador atual
            else if ((message.EndsWith("X") && PlayerIcon == 'X') || (message.EndsWith("O") && PlayerIcon == 'O'))
            {
                IsTime = true; // Permite o jogador atual jogar
                ReqMove();
            }
            else
            {
                Console.WriteLine("Aguarde a vez do seu oponente...");
                IsTime = false;
            }
        }
        else if (message == "X" || message == "O")
        {
            PlayerIcon = message[0];
            Console.WriteLine($"Você é o jogador: {PlayerIcon}");
            if (PlayerIcon == 'X')
            {
                IsTime = true;
                ReqMove();
            }
            else if (PlayerIcon == 'O')
            {
                IsTime = true;
                ReqMove();
            }
        }
        else if (message == "-1")
        {
            Console.WriteLine("Jogada inválida, tente novamente.");
            IsTime = true;
            ReqMove();
        }
        else if (message == "turn")
        {
            Console.WriteLine("Sua vez de jogar.");
            IsTime = true;
            ReqMove();
        }
        else if (message == "wait")
        {
            Console.WriteLine("Aguarde a vez do seu oponente...");
            IsTime = false;

        }
        else if (message.EndsWith("1") || message.EndsWith("2"))
        {
            Console.WriteLine($"Vitória! Jogador {message[1]} venceu!");
            DisplayBoard(message);
            Environment.Exit(0);
        }
        else if (message.EndsWith("3"))
        {
            Console.WriteLine("Empate!");
            DisplayBoard(message);
            Environment.Exit(0);
        }
    }

    private void ReqMove()
    {
        if (IsTime)
        {
            Console.WriteLine("Digite sua jogada (1-9): ");
            string move = Console.ReadLine();
            SendMessage(move);
        }
    }

    private void SendMessage(string message)
    {
        if (int.TryParse(message, out int move) && move >= 1 && move <= 9)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
            IsTime = false; // Aguarda a resposta do servidor
        }
        else
        {
            Console.WriteLine("Jogada inválida. Digite um número entre 1 e 9.");
            ReqMove(); // Solicitar nova jogada
        }
    }

    private void DisplayBoard(string boardState)
    {
        for (int i = 0; i < 9; i += 3)
        {
            Console.WriteLine("| {0} | {1} | {2} |", boardState[i], boardState[i + 1], boardState[i + 2]);
        }
    }

    static void Main(string[] args)
    {
        VelhaGameClient client = new VelhaGameClient();
    }
}