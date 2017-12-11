using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;



public class Emulator : MonoBehaviour {

	public File[] roms;
		byte[] memory;
		byte[] screen;
		Stack<ushort> stack;
		byte[] keys;
		ushort opcode;
		byte[] V; //16 Registers
		ushort I; //index register
		ushort pc; //program counter
		byte delay_timer;
		byte sound_timer;
		bool drawFlagBool;
		byte[] chip8_fontset =
		{
			0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
			0x20, 0x60, 0x20, 0x20, 0x70, // 1
			0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
			0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
			0x90, 0x90, 0xF0, 0x10, 0x10, // 4
			0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
			0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
			0xF0, 0x10, 0x20, 0x40, 0x40, // 7
			0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
			0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
			0xF0, 0x90, 0xF0, 0x90, 0x90, // A
			0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
			0xF0, 0x80, 0x80, 0x80, 0xF0, // C
			0xE0, 0x90, 0x90, 0x90, 0xE0, // D
			0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
			0xF0, 0x80, 0xF0, 0x80, 0x80  // F
		};

		public byte[] Screen
		{
			get
			{
				return screen;
			}
		}

		public Emulator()
		{
			screen = new byte[64 * 32];
			memory = new byte[4096];
			stack = new Stack<ushort>();
			keys = new byte[16];
			V = new byte[16];




		}


		public void initialize()
		{
			pc = 0x200;
			opcode = 0;
			I = 0;
			delay_timer = 0;
			sound_timer = 0;
			stack.Clear();

			clearScreen();

			for (int i = 0; i < V.Length; i++)
			{
				V[i] = 0;
			}
			for (int i = 0; i < memory.Length; i++)
			{
				memory[i] = 0;
			}

			//Load Fontset
			for (int i = 0; i < 80; i++)
			{
				memory[i] = chip8_fontset[i];
			}

			return;
		}

		private void clearScreen()
		{
			for (int i = 0; i < screen.Length; i++)
			{
				screen[i] = 0;
			}
			drawFlagBool = true;

		}

		public void loadGame(string file)
		{
			initialize();
			if (File.Exists(file))
			{
				FileStream stream = File.Open(file, FileMode.Open);
				byte[] buffer = new byte[stream.Length];
				stream.Read(buffer, 0, buffer.Length);

				const int PROGRAMMEMORYSTART = 512;
				for (int i = 0; i < buffer.Length; i++)
				{
					memory[i + PROGRAMMEMORYSTART] = buffer[i];
				}
			}

		}

		public void emulateCycle()
		{
			opcode = (ushort)(memory[pc] << 8 | memory[pc + 1]);

			switch (opcode & (0xF000))
			{
			case (0x0000):
				switch (opcode & (0x000F))
				{
				case (0X000):// 0x00E0: Clears the screen     
					clearScreen();
					drawFlagBool = true;
					break;
				case (0X00E):// 0x00EE: Returns from subroutine    
					pc = stack.Pop();
					pc += 2;
					break;
				}
				break;

			case (0x1000): // 1NNN	Jumps to address NNN.
				pc = (ushort)(opcode & 0x0FFF);
				break;

			case (0x2000): //Calls subroutine at NNN
				stack.Push(pc);
				pc = (ushort)(opcode & 0x0FFF);
				break;
			case (0x3000):// 3XNN	Skips the next instruction if VX equals NN. (Usually the next instruction is a jump to skip a code block)
				break;

			case (0x6000):// 6XNN Sets VX to NN.
				ushort x = (ushort)((opcode & (0X0F00)) >> 8);
				byte nn = (byte)(opcode & (0X00FF));
				V[x] = nn;
				pc += 2;
				break;
			case (0xF000):
				switch (opcode & (0x00FF))
				{
				case (0x0007):
					break;
				case (0x000A):
					break;
				case (0x0015):
					break;
				case (0x0018):
					break;
				case (0x001E):
					break;
				case (0x0029):
					break;
				case (0x0033):
					break;
				case (0x0055):
					break;
				case (0x0065):
					break;
				}
				break;
			case (0x8000):
				switch (opcode & (0x000F))
				{
				case (0x0000):
					break;
				case (0x0001):
					break;
				case (0x0002):
					break;
				case (0x0003):
					break;
				case (0x0004):
					break;
				case (0x0005):
					break;
				case (0x0006):
					break;
				case (0x0007):
					break;
				case (0x000E):
					break;
				}
				break;

			case (0xA000):  // ANNN: Sets I to the address NNN
				I = (ushort)(opcode & (0x0FFF));
				pc += 2;
				break;

			case (0xD000):  //DXYN: Draws a sprite at coordinate (VX, VY) that has a width of 8 pixels and a height of N pixels. 
				//Each row of 8 pixels is read as bit-coded starting from memory location I; I value doesn’t change after the execution of this instruction. 
				//As described above, VF is set to 1 if any screen pixels are flipped from set to unset when the sprite is drawn, and to 0 if that doesn’t happen

				ushort vx = V[(opcode & 0x0F00) >> 8];
				ushort vy = V[(opcode & 0x00F0) >> 4];
				ushort height = (ushort)(opcode & 0x000F);
				ushort pixel;

				V[0xF] = 0;
				for (int yline = 0; yline < height; yline++)
				{
					pixel = memory[I + yline];
					for (int xline = 0; xline < 8; xline++)
					{
						if ((pixel & (0x80 >> xline)) != 0)
						{
							if (screen[(vx + xline + ((vy + yline) * 64))] == 1)
								V[0xF] = 1;
							screen[vx + xline + ((vy + yline) * 64)] ^= 1;
						}

					}
				}
				drawFlagBool = true;
				pc += 2;
				break;

			default:
				System.Console.WriteLine("Unbekannter opcode: {0:X}", opcode);
				Console.WriteLine("Hex: {0:X}", (opcode & (0x0FFF)));
				pc += 2;
				break;


			}

			if (delay_timer > 0)
			{
				--delay_timer;
			}
			if (sound_timer > 0)
			{
				if (sound_timer == 1)
				{
					Console.WriteLine("BEEEP");
					--sound_timer;
				}
			}


		}
		public bool drawFlag()
		{
			drawFlagBool = false;
			return drawFlagBool;

		}

		public void setKeys()
		{
			return;

		}


	}
