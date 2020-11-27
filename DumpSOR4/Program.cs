using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

using CommonLib;

using BeatThemAll;
using BeatThemAll.MetaGame;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DumpSOR4 {

	internal class Program {
		
		public static void Main ( string[] args ) {
			
			string dataFolder = args[ 0 ];
			string saveFolder = args[ 1 ];
			
			// use this to ref SOR4.dll
			var sd = new SpriteData ();

			reflection.delegate_serialize = ( Action< Stream, object > )typeof ( program )
																		.GetMethod ( "delegate_serialize",
																			BindingFlags.NonPublic |
																			BindingFlags.Static )
																		.CreateDelegate (
																			typeof ( Action< Stream, object > ) );
			reflection.delegate_deserialize =
				( Func< Stream, object, Type, Action< string >, object > )typeof ( program )
																		  .GetMethod ( "delegate_deserialize",
																			  BindingFlags.NonPublic |
																			  BindingFlags.Static )
																		  .CreateDelegate (
																			  typeof ( Func< Stream, object, Type,
																				  Action< string >, object > ) );
			reflection.delegate_deep_clone = ( Func< object, object > )typeof ( program )
																	   .GetMethod ( "delegate_deep_clone",
																		   BindingFlags.NonPublic |
																		   BindingFlags.Static )
																	   .CreateDelegate (
																		   typeof ( Func< object, object > ) );

			utils.showErrorReportDelegate = OnShowErrorReportDelegate;
			utils.set_as_main_thread ();

			platform.set_data_folder ( dataFolder );

			// Deserialize AssetRef
			InitAssetCache ();

			// Create Graphic Context
			xna.initialize ( GameUpdate, GameDraw, GameShutdown, false, true );
			global_content.load ();

			asset_cache.canLoadMoreThanOneTexturePerFrame = true;
			asset_cache.start_background_loading ( new [] { new AssetId_s ( "MetaGameConfig", typeof ( MetaGameConfigData ) ) }, false );
			while ( !asset_cache.background_loading_finished () ) {
				asset_cache.update_on_main_thread ( true );
				// platform.update();
				Thread.Sleep ( 10 );
				Thread.Yield ();
			}
			asset_cache.canLoadMoreThanOneTexturePerFrame = false;
			
			Console.WriteLine ( "asset_cache loading finished." );

			DumpAllAssetsTo ( saveFolder );

			Environment.Exit ( 0 );
			
			// if ( xna.game != null ) {
			// 	using ( xna.game ) {
			// 		xna.game.Run ();
			// 	}
			// }
		}


		private static void GameUpdate () {}
		
		private static void GameDraw ( float deltaTime ) {
			xna.game.GraphicsDevice.Clear ( Color.Black );
		}

		private static void GameShutdown () {}


		private static void OnShowErrorReportDelegate ( Exception e ) {
			// xna.show_message_box_ok ( "ShowErrorReport", e.Message );
			Console.WriteLine ( e.Message );
		}


		private static void InitAssetCache () {
			using ( Stream input =
				platform.get_decompressor ( File.OpenRead ( Path.Combine ( platform.get_data_folder (), "bigfile" ) ) )
			) {
				BinaryReader binaryReader = new BinaryReader ( input, Encoding.Unicode );
				for ( int num = binaryReader.ReadInt32 (); num > 0; num-- ) {
					string name      = binaryReader.ReadString ();
					Type   type      = reflection.get_type_by_full_name_without_namespace ( name );
					string assetPath = binaryReader.ReadString ();
					int    count     = binaryReader.ReadInt32 ();
					byte[] byteArray = binaryReader.ReadBytes ( count );
					object obj       = reflection.bin_deserialize ( byteArray, type );

					if ( obj is IAssetEvents ) {
						( ( IAssetEvents )obj ).on_asset_modified ( assetPath );
					}

					// Console.WriteLine ( $"{name}: {assetPath}" );

					asset_cache.add_asset_object ( new AssetId_s ( assetPath, type ), obj );
				}
			}

			AssetDependencyCacheData assetDependencyCacheData =
				asset_cache.get< AssetDependencyCacheData > ( "dependencies" );
			assetDependencyCacheData.set_in_cache ();
			object obj2;
			asset_cache.assetById.TryRemove ( new AssetId_s ( "dependencies", typeof ( AssetDependencyCacheData ) ),
				out obj2 );
			GC.Collect ();
			asset_cache.textureBigFileArray = new asset_cache.TextureBigFile[2];
			asset_cache.textureBigFileArray[ 0 ] =
				open_texture_bigfile_and_table ( "texture_table", "textures" );
			asset_cache.textureBigFileArray[ 1 ] =
				open_texture_bigfile_and_table ( "texture_table02", "textures02" );
		}


		private static void DumpAllAssetsTo ( string savePath ) {
			
			Dictionary<string, List<Texture2D>> textures = new Dictionary<string, List<Texture2D>>();

			Action< string, Texture2D > addTextureIfNotExist = (baseAssetPath, texture2D) => {
				if ( !textures.ContainsKey ( baseAssetPath ) ) {
					textures.Add ( baseAssetPath, new List< Texture2D >() );
				}
				if ( !textures[ baseAssetPath ].Contains ( texture2D ) ) {
					textures[ baseAssetPath ].Add ( texture2D );
				}
			};
			
			foreach ( var kv in asset_cache.assetById ) {
				
				if ( kv.Key.type == typeof ( SpriteData ) ) {
					SpriteData.Quad_s[] quadArray = ( ( SpriteData )kv.Value ).quadArray;
					if ( quadArray != null ) {
						for ( int i = 0; i < quadArray.Length; i++ ) {
							addTextureIfNotExist ( kv.Key.assetPath, quadArray[ i ].texture.Value );
						}
						
						// for Debug
						// break;
					}
					
					// for ( int i = 0; i < textures.Count; i++ ) {
					// 	DumpTexture2D ( textures[ i ], savePath, kv.Key.assetPath );
					// }
				}
				
				if ( kv.Key.type == typeof ( AnimatedSpriteData ) ) {
					
				}
				
				if ( kv.Key.type == typeof ( CharacterData ) ) {
					
				}
				
				if ( kv.Key.type == typeof ( LevelData ) ) {
					
				}
			}

			foreach ( var kv in textures ) {
				for ( int i = 0; i < kv.Value.Count; i++ ) {
					DumpTexture2D ( kv.Value[ i ], savePath, kv.Key );
				}
			}
		}


		private static void DumpTexture2D ( Texture2D texture, string savePath, string assetPath ) {
			if ( texture == null ) {
				return;
			}

			AffirmAssetFolder ( savePath, assetPath );
			
			string assetDir = Path.GetDirectoryName ( assetPath );
			string filePath = Path.Combine ( savePath, assetDir, Path.GetFileNameWithoutExtension ( assetPath ) + ".png" );
			// string filePath = Path.Combine ( savePath, assetDir, texture.Name );

			using ( Stream stream = File.Create ( filePath ) ) {
				texture.SaveAsPng ( stream, texture.Width, texture.Height );
				stream.Flush ();
			}

			Console.WriteLine ( $"DumpTexture2D: {filePath}" );
		}
		

		private static void AffirmAssetFolder ( string savePath, string assetPath ) {
			string[] elmArray = assetPath.Split ( '/' );
			
			// the last element is filename, so drop it
			for ( int i = 0; i < elmArray.Length - 1; i++ ) {
				string path = savePath;
				for ( int j = 0; j <= i; j++ ) {
					path = Path.Combine ( path, elmArray[ j ] );
				}

				if ( !Directory.Exists ( path ) ) {
					Directory.CreateDirectory ( path );
					
					Console.WriteLine ( $"Create Dir: {path}" );
				}
			}
		}
		

		private static asset_cache.TextureBigFile open_texture_bigfile_and_table (
			string tableName, string bigFileName ) {
			asset_cache.TextureBigFile textureBigFile = new asset_cache.TextureBigFile ();
			using ( BinaryReader binaryReader =
				new BinaryReader (
					new FileStream ( Path.Combine ( platform.get_data_folder (), tableName ), FileMode.Open ),
					Encoding.Unicode ) ) {
				asset_cache.LengthAndPos_s value = default ( asset_cache.LengthAndPos_s );
				while ( binaryReader.BaseStream.Position < binaryReader.BaseStream.Length ) {
					string key = binaryReader.ReadString ();
					value.position = binaryReader.ReadInt64 ();
					value.length   = binaryReader.ReadInt32 ();
					textureBigFile.textureLengthAndPosByAssetPath.Add ( key, value );
				}
			}

			textureBigFile.textureBigFileStream =
				new FileStream ( Path.Combine ( platform.get_data_folder (), bigFileName ), FileMode.Open );
			return textureBigFile;
		}


		public static void PrintAllAssets ( string dataFolder ) {
			using ( Stream input =
				platform.get_decompressor ( File.OpenRead ( Path.Combine ( dataFolder, "bigfile" ) ) )
			) {
				BinaryReader binaryReader = new BinaryReader ( input, Encoding.Unicode );
				for ( int num = binaryReader.ReadInt32 (); num > 0; num-- ) {
					string name      = binaryReader.ReadString ();
					Type   type      = reflection.get_type_by_full_name_without_namespace ( name );
					string assetPath = binaryReader.ReadString ();
					int    count     = binaryReader.ReadInt32 ();
					byte[] byteArray = binaryReader.ReadBytes ( count );
					object obj       = reflection.bin_deserialize ( byteArray, type );
					// if ( obj is IAssetEvents ) {
					// 	( ( IAssetEvents )obj ).on_asset_modified ( assetPath );
					// }

					// if ( type == typeof ( MetaGameConfigData ) ) {
					// 	var configData = obj as MetaGameConfigData;
					// 	configData.enableOnline4P = true;
					// }

					Console.WriteLine ( $"{name}: {assetPath}" );

					// add_asset_object ( assetPath, type, obj );
				}
			}
		}
	}

}