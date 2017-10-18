using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class TablePostProcessor : AssetPostprocessor {
	static List<string> __path_list = null;
	static List<string> _path_list {
		get {
			if( __path_list == null ) {
				string [] paths = { "TableData.cs" };
				__path_list = new List<string>( paths );
			}
			return __path_list;
		}
	}

	static List<string> __header_list = null;
	static List<string> _header_list {
		get {
			if( __header_list == null ) {
				__header_list = new List<string>();

				__header_list.Add( "using UnityEngine;" );
				__header_list.Add( "using System;" );
				__header_list.Add( "" );
			}
			return __header_list;
		}
	}

    static List<string> _ImportFilePathList = new List<string>();

	static void OnPostprocessAllAssets( string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths ) {
		_ImportFilePathList.Clear();

		if( (importedAssets != null) && (importedAssets.Length > 0) ) {
			for( int i=0; i<importedAssets.Length; i++ ) {
				string str = importedAssets[i];
				OnImportedAssets( str );
			}
		}

		if( (deletedAssets != null) && (deletedAssets.Length > 0) ) {
			for( int i=0; i<deletedAssets.Length; i++ ) {
				string str = deletedAssets[i];
				OnDeleteAssets( str );
			}
		}

		OnImportXmls( importedAssets, deletedAssets );

		if( _ImportFilePathList.Count > 0 ) {
			AssetDatabase.Refresh();
		}
	}

	static List<string> _FileList = new List<string>();

	static void OnImportXmls( string[] importedAssets, string[] deletedAssets ) {
		bool is_xmls = false;
		string[] assets;

		if( ((assets = importedAssets) != null) && (assets.Length > 0) ) {
			for( int i=0; i<assets.Length; i++ ) {
				string path_name = Path.GetDirectoryName( assets[i] );
				if( path_name.Contains( "Assets/Resources/Xmls" ) == false ) {
					continue;
				}
				is_xmls = true;
			}
		}

		if( (is_xmls == false) && ((assets = deletedAssets) != null) && (assets.Length > 0) ) {
			for( int i=0; i<assets.Length; i++ ) {
				string path_name = Path.GetDirectoryName( assets[i] );
				if( path_name.Contains( "Assets/Resources/Xmls" ) == false ) {
					continue;
				}
				is_xmls = true;
			}
		}

		if( is_xmls == true ) {
			_FileList.Clear();
			jUtil.GetFiles( _FileList, "Assets/Resources/Xmls", "*.xml" );

			Dictionary<string,List<string>> dic = new Dictionary<string, List<string>>();

			string [] strs;
			for( int i=0; i<_FileList.Count; i++ ) {
				strs = _FileList[i].Split( '/' );
				if( (strs != null) && (strs.Length == 3) ) {
					if( dic.ContainsKey( strs[1] ) == false ) {
						dic.Add( strs[1], new List<string>() );
					}
					if( dic[ strs[1] ].Contains( strs[2] ) == false ) {
						dic[ strs[1] ].Add( strs[2] );
					}
				}
			}

			List<string> strlist = new List<string>();

			strlist.Add( "public class XmlList {" );

			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			foreach( KeyValuePair<string,List<string>> each in dic ) {
				sb.Append( "\tpublic static string [] _" );
				sb.Append( each.Key );
				sb.Append( " = { " );

				List<string> list = each.Value;
				for( int i=0; i<list.Count; i++ ) {
					if( i > 0 ) {
						sb.Append( ",\r\n\t\t\t\t\t\t\t\t\t\t" );
					}
					sb.Append( "\"" );
					sb.Append( list[i] );
					sb.Append( "\"" );
				}

				sb.Append( " };" );

				strlist.Add( sb.ToString() );
				sb.Remove( 0, sb.Length );

				strlist.Add( "" );
			}

			strlist.Add( "}" );

			CreateFile( "Assets/Scripts/XmlList.cs", strlist );
		}
	}

	static void OnImportedAssets( string path ) {
		string path_name = Path.GetDirectoryName( path );
		if( path_name.Contains( "Assets/Scripts/Data" ) == false ) {
			return;
		}

		string file_name = Path.GetFileName( path );
		if( _path_list.Contains( file_name ) == false ) {
			return;
		}

		string [] reads = File.ReadAllLines( path );
		if( (reads == null) || (reads.Length == 0) ) {
			return;
		}

		ImportedAssets( path_name + "/Binary_"  + file_name, reads );
	}

	public enum PACK_TYPE {
		INIT = 0,
		START,
		END
	}

	static void ImportedAssets( string path, string [] lines ) {
		PACK_TYPE pack_type = PACK_TYPE.END;
		string line, header_name = "", class_name = "", header_line;
		string [] splits;
		List<string> split_list = new List<string>();
		List<string> var_list = new List<string>();
		List<string> code_list = new List<string>();
		bool derived = false;
		string [] sep_basic = { "\t", " ", ":" };

		for( int i=0; i<lines.Length; i++ ) {
			line = lines[i].Trim();

			switch( pack_type ) {
			case PACK_TYPE.END:
				if( line.Contains( "//<Binary_Pack>" ) == true ) {
					pack_type = PACK_TYPE.INIT;
					derived = false;
					header_name = "";
					class_name = "";
					var_list.Clear();
				}
				break;
			case PACK_TYPE.INIT:
                if( line.Contains( "//<Binary_Pack_Start>" ) == true ) {
					pack_type = PACK_TYPE.START;
				} else if( class_name.Length == 0 ) {
					header_line = line;
					if( header_line.Contains( "where" ) == true ) {
						header_line = header_line.Substring( 0, header_line.IndexOf( "where" ) + 1 );
					}

					splits = header_line.Split( sep_basic, System.StringSplitOptions.RemoveEmptyEntries );

					split_list.Clear();
					split_list.AddRange( splits );

					int finds = 0;
					string cls_name = "";
					for( int j=0; j<split_list.Count; j++ ) {
						if( split_list[j].StartsWith( "//" ) == true ) {
							break;
						}

						switch( split_list[j] ) {
						case "public":
							break;
						case "partial":
							break;
						case "class":
							cls_name = split_list[ j + 1 ];
							break;
						default:
							continue;
						}

						finds++;
					}
					if( finds == 3 ) {
						if( header_line.Contains( ":" ) == true ) {
							derived = true;
						}
						header_name = line;
						class_name = cls_name;
					}
				}
				break;
			case PACK_TYPE.START:
                if( line.Contains( "//<Binary_Pack_End>" ) == true ) {
					pack_type = PACK_TYPE.END;

					ChangeData( ref code_list, derived, header_name, class_name, var_list );

					var_list.Clear();
				} else {
					var_list.Add( line );
				}
				break;
			}
		}

		if( code_list.Count > 0 ) {
			CreateFile( path, code_list );
		}
	}

	static void ChangeData( ref List<string> code_list, bool derived, string header_name, string class_name, List<string> var_list ) {
		VAR_TYPE var_type, sub_type;
		string type_name;
		string line, ret;
		int index;
		string [] split_base, split_detail;
		string [] sep_basic = { "\t", " " };
		string [] sep_detail = { "List<", ">", "\t", " " };

		string read_name, write_name, copy_name;
		List<string> read_list = new List<string>();
		List<string> write_list = new List<string>();
		List<string> copy_list = new List<string>();

		for( int i=0; i<var_list.Count; i++ ) {
			line = var_list[i].Trim();

			line = line.Replace( ";", "" );
			line = line.Replace( "public", "" );

			if( (index = line.IndexOf( "//" )) >= 0 ) {
				line = line.Substring( 0, index );
			}

			if( (index = line.IndexOf( "=" )) >= 0 ) {
				line = line.Substring( 0, index );
			}

			if( (line.IndexOf( '[' ) == 0) && (line.IndexOf( ']' ) == (line.Length - 1)) ) {
				continue;
			}

			if( line.Length < 1 ) {
				continue;
			}

			split_base = line.Split( sep_basic, System.StringSplitOptions.RemoveEmptyEntries );
			if( (split_base == null) || (split_base.Length <= 0) ) {
				continue;
			}

			type_name = GetTypeData( split_base[0], out var_type, out sub_type ).ToString().Replace( "UnityEngine.", "" ).Replace( "System.", "" );

			switch( type_name ) {
			case "GameObject":
			case "AudioClip":
				continue;
			}

			read_name = "";
			write_name = "";
			copy_name = "";

			ret = line.Replace( " ", "" );
			if( ret.Contains( "[]" ) == true ) {
				// 배열 처리
				ret = line.Replace( "[", "" );
				ret = ret.Replace( "]", "" );

				split_base = ret.Split( sep_basic, System.StringSplitOptions.RemoveEmptyEntries );
				split_detail = split_base[0].Split( sep_detail, System.StringSplitOptions.RemoveEmptyEntries );

				type_name = GetTypeData( split_detail[0], out var_type, out sub_type ).ToString().Replace( "UnityEngine.", "" ).Replace( "System.", "" );

				switch( var_type ) {
				case VAR_TYPE.CAST:
					read_name = _read_array_type[2];
					write_name = _write_array_type[2];
					copy_name = _copy_array_type[2];
					break;
				case VAR_TYPE.CLASS:
					read_name = _read_array_type[1];
					write_name = _write_array_type[1];
					copy_name = _copy_array_type[1];
					break;
				default:
					read_name = _read_array_type[0];
					write_name = _write_array_type[0];
					copy_name = _copy_array_type[0];
					break;
				}
			} else if( split_base[0].StartsWith( "List<" ) == true ) {
				// 리스트 처리
				split_detail = split_base[0].Split( sep_detail, System.StringSplitOptions.RemoveEmptyEntries );
				if( split_detail[0] == "T" ) {
					var_type = VAR_TYPE.CLASS;
				} else {
					type_name = GetTypeData( split_detail[0], out var_type, out sub_type ).ToString().Replace( "UnityEngine.", "" ).Replace( "System.", "" );
				}

				switch( var_type ) {
				case VAR_TYPE.CAST:
					read_name = _read_list_type[2];
					write_name = _write_list_type[2];
					copy_name = _copy_list_type[2];
					break;
				case VAR_TYPE.CLASS:
					read_name = _read_list_type[1];
					write_name = _write_list_type[1];
					copy_name = _copy_list_type[1];
					break;
				default:
					read_name = _read_list_type[0];
					write_name = _write_list_type[0];
					copy_name = _copy_list_type[0];
					break;
				}
			} else {
				split_detail = split_base[0].Split( sep_detail, System.StringSplitOptions.RemoveEmptyEntries );

				switch( var_type ) {
				case VAR_TYPE.CAST:
					read_name = _read_variable_type[2];
					write_name = _write_variable_type[2];
					copy_name = _copy_variable_type[2];
					break;
				case VAR_TYPE.CLASS:
					read_name = _read_variable_type[1];
					write_name = _write_variable_type[1];
					copy_name = _copy_variable_type[1];
					break;
				default:
					read_name = _read_variable_type[0];
					write_name = _write_variable_type[0];
					copy_name = _copy_variable_type[0];
					break;
				}
			}

			read_name = read_name.Replace( "{0}", split_base[1] ).Replace( "{1}", type_name ).Replace( "{2}", split_detail[0] );
			if( read_name.Length > 0 ) {
				read_list.Add( read_name );
			}

			write_name = write_name.Replace( "{0}", split_base[1] );
			if( write_name.Length > 0 ) {
				write_list.Add( write_name );
			}

			copy_name = copy_name.Replace( "{0}", split_base[1] ).Replace( "{2}", split_detail[0] );
			if( copy_name.Length > 0 ) {
				copy_list.Add( copy_name );
			}
		}

		code_list.Add( header_name );

			// UnpackData
			if( derived == true ) {
				code_list.Add( "\tpublic override void UnpackData( jSerializerReader sr ) {" );
				code_list.Add( "\t\tbase.UnpackData( sr );" );
			} else {
				code_list.Add( "\tpublic virtual void UnpackData( jSerializerReader sr ) {" );
			}

			code_list.AddRange( read_list );

			code_list.Add( "\t}" );
			code_list.Add( "" );

			// PackData
			if( derived == true ) {
				code_list.Add( "\tpublic override void PackData( jSerializerWriter sw ) {" );
				code_list.Add( "\t\tbase.PackData( sw );" );
			} else {
				code_list.Add( "\tpublic virtual void PackData( jSerializerWriter sw ) {" );
			}

			code_list.AddRange( write_list );

			code_list.Add( "\t}" );
			code_list.Add( "" );

			// CopyFrom
			if( derived == true ) {
				code_list.Add( "\tpublic override void CopyFrom( object source ) {".Replace("{0}",class_name) );
				code_list.Add( "\t\t{0} src = source as {0};".Replace("{0}",class_name) );

				code_list.Add( "\t\tbase.CopyFrom( src );" );
			} else {
				code_list.Add( "\tpublic virtual void CopyFrom( object source ) {".Replace( "{0}", class_name ) );
				code_list.Add( "\t\t{0} src = source as {0};".Replace("{0}",class_name) );
			}

			code_list.AddRange( copy_list );

			code_list.Add( "\t}" );

		code_list.Add( "}" );
		code_list.Add( "" );
	}

	static void CreateFile( string path, List<string> code_list ) {
		code_list.InsertRange( 0, _header_list );

		File.WriteAllLines( path, code_list.ToArray() );

		_ImportFilePathList.Add( path );
		Debug.Log( "Rebuild : " + path );
	}

	static void OnDeleteAssets( string path ) {
		string path_name = Path.GetDirectoryName( path );
		if( path_name.Contains( "Assets/Scripts/Data" ) == false ) {
			return;
		}

		string file_name = Path.GetFileName( path );
		if( _path_list.Contains( file_name ) == false ) {
			return;
		}

		DeleteAssets( path_name + "/Binary_"  + file_name );
	}

	static void DeleteAssets( string path ) {
		if( File.Exists( path ) == false ) {
			return;
		}

		File.Delete( path );

		_ImportFilePathList.Add( path );
		Debug.Log( "Delete : " + path );
	}

	public enum VAR_TYPE {
		BASIC = 0,
		CLASS,
		STRUCT,
		CAST
	}

	public static System.Type GetTypeData( string name, out VAR_TYPE var_type, out VAR_TYPE sub_type ) {
		var_type = VAR_TYPE.BASIC;
		sub_type = VAR_TYPE.BASIC;

		System.Type t = System.Type.GetType( name );
		if( name == "T" ) {
			var_type = VAR_TYPE.CLASS;
			return null;
		}
		if( t != null ) {
			if( t.IsClass == true ) {
				var_type = VAR_TYPE.CLASS;
			}
			return t;
		}
        
		switch( name ) {
		case "bool":
			return typeof( bool );
		case "byte":
			return typeof( byte );
		case "sbyte":
			return typeof( sbyte );
		case "short":
			return typeof( short );
		case "int":
			return typeof( int );
		case "char":
			return typeof( char );
		case "string":
			return typeof( string );
		case "float":
			return typeof( float );
		case "double":
			return typeof( double );

		case "DateTime":
			var_type = VAR_TYPE.STRUCT;
			return typeof( System.DateTime );
		case "Vector3":
			var_type = VAR_TYPE.STRUCT;
			return typeof( Vector3 );
		case "Vector4":
			var_type = VAR_TYPE.STRUCT;
			return typeof( Vector4 );
		case "Color":
			var_type = VAR_TYPE.STRUCT;
			return typeof( Color );
		case "AnimationCurve":
			sub_type = VAR_TYPE.CLASS;
			return typeof( AnimationCurve );

		// Basic
		case "TableData":
			var_type = VAR_TYPE.CLASS;
			return typeof( TableData );       
		case "GameObject":
			var_type = VAR_TYPE.CLASS;
			return typeof( GameObject );
		case "AudioClip":
			var_type = VAR_TYPE.CLASS;
			return typeof( AudioClip );
		}

		var_type = VAR_TYPE.CAST;
		return typeof( int );
	}

	////////////////////////////////////////////////////////////////////////////////////////////
	// READ
	static string [] _read_array_type = {
											"\t\t" + "int count_{0} = sr.ReadInt32();" + "\r\n" +
											"\t\t" + "{0} = new {2}[count_{0}];" + "\r\n" +
											"\t\t" + "for( int i=0; i<count_{0}; i++ ) {" + "\r\n" +
											"\t\t\t" + "{0}[i] = sr.Read{1}();" + "\r\n" +
											"\t\t" + "}"
										,
											"\t\t" + "int count_{0} = sr.ReadInt32();" + "\r\n" +
											"\t\t" + "{0} = new {2}[count_{0}];" + "\r\n" +
											"\t\t" + "for( int i=0; i<count_{0}; i++ ) {" + "\r\n" +
											"\t\t\t" + "{2} data_{0} = new {2}();" + "\r\n" +
											"\t\t\t" + "data_{0}.UnpackData( sr );" + "\r\n" +
											"\t\t\t" + "{0}[i] = data_{0};" + "\r\n" +
											"\t\t" + "}"
										,
											"\t\t" + "int count_{0} = sr.ReadInt32();" + "\r\n" +
											"\t\t" + "{0} = new {2}[count_{0}];" + "\r\n" +
											"\t\t" + "for( int i=0; i<count_{0}; i++ ) {" + "\r\n" +
											"\t\t\t" + "{0}[i] = ({2})sr.Read{1}();" + "\r\n" +
											"\t\t" + "}"
										};

	static string [] _read_list_type = {
											"\t\t" + "int count_{0} = sr.ReadInt32();" + "\r\n" +
											"\t\t" + "{0}.Clear();" + "\r\n" +
											"\t\t" + "for( int i=0; i<count_{0}; i++ ) {" + "\r\n" +
											"\t\t\t" + "{0}.Add( sr.Read{1}() );" + "\r\n" +
											"\t\t" + "}"
										,
											"\t\t" + "int count_{0} = sr.ReadInt32();" + "\r\n" +
											"\t\t" + "{0}.Clear();" + "\r\n" +
											"\t\t" + "for( int i=0; i<count_{0}; i++ ) {" + "\r\n" +
											"\t\t\t" + "{2} data_{0} = new {2}();" + "\r\n" +
											"\t\t\t" + "data_{0}.UnpackData( sr );" + "\r\n" +
											"\t\t\t" + "{0}.Add( data_{0} );" + "\r\n" +
											"\t\t" + "}"
										,
											"\t\t" + "if( {0} == null ) {0} = new List<{2}>();" + "\r\n" +
											"\t\t" + "int count_{0} = sr.ReadInt32();" + "\r\n" +
											"\t\t" + "{0}.Clear();" + "\r\n" +
											"\t\t" + "for( int i=0; i<count_{0}; i++ ) {" + "\r\n" +
											"\t\t\t" + "{0}.Add( ({2})sr.Read{1}() );" + "\r\n" +
											"\t\t" + "}"
										};

	static string [] _read_variable_type = {
											"\t\t{0} = sr.Read{1}();"
										,
											"\t\tif( {0} == null ) {0} = new {2}();\r\n" + 
											"\t\t{0}.UnpackData( sr );"
										,
											"\t\t{0} = ({2})sr.Read{1}();"
										};
	////////////////////////////////////////////////////////////////////////////////////////////

	////////////////////////////////////////////////////////////////////////////////////////////
	// WRITE
	static string [] _write_array_type = {
											"\t\t" + "if( {0} == null ) {" + "\r\n" +
											"\t\t\t" + "sw.Write( 0 );" + "\r\n" +
											"\t\t" + "} else {" + "\r\n" +
											"\t\t\t" + "sw.Write( (int){0}.Length );" + "\r\n" +
											"\t\t\t" + "for( int i=0; i<{0}.Length; i++ ) {" + "\r\n" +
											"\t\t\t\t" + "sw.Write( {0}[i] );" + "\r\n" +
											"\t\t\t" + "}" + "\r\n" +
											"\t\t" + "}"
										,
											"\t\t" + "if( {0} == null ) {" + "\r\n" +
											"\t\t\t" + "sw.Write( 0 );" + "\r\n" +
											"\t\t" + "} else {" + "\r\n" +
											"\t\t\t" + "sw.Write( (int){0}.Length );" + "\r\n" +
											"\t\t\t" + "for( int i=0; i<{0}.Length; i++ ) {" + "\r\n" +
											"\t\t\t\t" + "{0}[i].PackData( sw );" + "\r\n" +
											"\t\t\t" + "}" + "\r\n" +
											"\t\t" + "}"
										,
											"\t\t" + "if( {0} == null ) {" + "\r\n" +
											"\t\t\t" + "sw.Write( 0 );" + "\r\n" +
											"\t\t" + "} else {" + "\r\n" +
											"\t\t\t" + "sw.Write( (int){0}.Length );" + "\r\n" +
											"\t\t\t" + "for( int i=0; i<{0}.Length; i++ ) {" + "\r\n" +
											"\t\t\t\t" + "sw.Write( (int){0}[i] );" + "\r\n" +
											"\t\t\t" + "}" + "\r\n" +
											"\t\t" + "}"
										};

	static string [] _write_list_type = {
											"\t\t" + "if( {0} == null ) {" + "\r\n" +
											"\t\t\t" + "sw.Write( 0 );" + "\r\n" +
											"\t\t" + "} else {" + "\r\n" +
											"\t\t\t" + "sw.Write( (int){0}.Count );" + "\r\n" +
											"\t\t\t" + "for( int i=0; i<{0}.Count; i++ ) {" + "\r\n" +
											"\t\t\t\t" + "sw.Write( {0}[i] );" + "\r\n" +
											"\t\t\t" + "}" + "\r\n" +
											"\t\t" + "}"
										,
											"\t\t" + "if( {0} == null ) {" + "\r\n" +
											"\t\t\t" + "sw.Write( 0 );" + "\r\n" +
											"\t\t" + "} else {" + "\r\n" +
											"\t\t\t" + "sw.Write( (int){0}.Count );" + "\r\n" +
											"\t\t\t" + "for( int i=0; i<{0}.Count; i++ ) {" + "\r\n" +
											"\t\t\t\t" + "{0}[i].PackData( sw );" + "\r\n" +
											"\t\t\t" + "}" + "\r\n" +
											"\t\t" + "}"
										,
											"\t\t" + "if( {0} == null ) {" + "\r\n" +
											"\t\t\t" + "sw.Write( 0 );" + "\r\n" +
											"\t\t" + "} else {" + "\r\n" +
											"\t\t\t" + "sw.Write( (int){0}.Count );" + "\r\n" +
											"\t\t\t" + "for( int i=0; i<{0}.Count; i++ ) {" + "\r\n" +
											"\t\t\t\t" + "sw.Write( (int){0}[i] );" + "\r\n" +
											"\t\t\t" + "}" + "\r\n" +
											"\t\t" + "}"
										};

	static string [] _write_variable_type = {
											"\t\tsw.Write( {0} );"
										,
											"\t\tif( {0} != null ) {" + "\r\n" + 
											"\t\t\t{0}.PackData( sw );" + "\r\n" + 
											"\t\t}"
										,
											"\t\tsw.Write( (int){0} );"
										};
	////////////////////////////////////////////////////////////////////////////////////////////

	////////////////////////////////////////////////////////////////////////////////////////////
	// COPY
	static string [] _copy_array_type = {
											"\t\t" + "if( src.{0} == null ) {" + "\r\n" +
											"\t\t\t" + "{0} = null;" + "\r\n" +
											"\t\t" + "} else {" + "\r\n" +
											"\t\t\t" + "int count_{0} = src.{0}.Length;" + "\r\n" +
											"\t\t\t" + "{0} = new {2}[count_{0}];" + "\r\n" +
											"\t\t\t" + "for( int i=0; i<count_{0}; i++ ) {" + "\r\n" +
											"\t\t\t\t" + "{0}[i] = src.{0}[i];" + "\r\n" +
											"\t\t\t" + "}" + "\r\n" +
											"\t\t" + "}"
										,
											"\t\t" + "if( src == null ) {" + "\r\n" +
											"\t\t\t" + "{0} = null;" + "\r\n" +
											"\t\t" + "} else {" + "\r\n" +
											"\t\t\t" + "int count_{0} = src.{0}.Length;" + "\r\n" +
											"\t\t\t" + "{0} = new {2}[count_{0}];" + "\r\n" +
											"\t\t\t" + "for( int i=0; i<count_{0}; i++ ) {" + "\r\n" +
											"\t\t\t\t" + "{2} data_{0} = new {2}();" + "\r\n" +
											"\t\t\t\t" + "data_{0}.CopyFrom( src.{0}[i] );" + "\r\n" +
											"\t\t\t\t" + "{0}[i] = data_{0};" + "\r\n" +
											"\t\t\t" + "}" + "\r\n" +
											"\t\t" + "}"
										,
											"\t\t" + "if( src.{0} == null ) {" + "\r\n" +
											"\t\t\t" + "{0} = null;" + "\r\n" +
											"\t\t" + "} else {" + "\r\n" +
											"\t\t\t" + "int count_{0} = src.{0}.Length;" + "\r\n" +
											"\t\t\t" + "{0} = new {2}[count_{0}];" + "\r\n" +
											"\t\t\t" + "for( int i=0; i<count_{0}; i++ ) {" + "\r\n" +
											"\t\t\t\t" + "{0}[i] = src.{0}[i];" + "\r\n" +
											"\t\t\t" + "}" + "\r\n" +
											"\t\t" + "}"
										};

	static string [] _copy_list_type = {
											"\t\t" + "if( src.{0} == null ) {" + "\r\n" +
											"\t\t\t" + "{0} = null;" + "\r\n" +
											"\t\t" + "} else {" + "\r\n" +
											"\t\t\t" + "int count_{0} = src.{0}.Count;" + "\r\n" +
											"\t\t\t" + "{0}.Clear();" + "\r\n" +
											"\t\t\t" + "for( int i=0; i<count_{0}; i++ ) {" + "\r\n" +
											"\t\t\t\t" + "{0}.Add( src.{0}[i] );" + "\r\n" +
											"\t\t\t" + "}" + "\r\n" +
											"\t\t" + "}"
										,
											"\t\t" + "if( src.{0} == null ) {" + "\r\n" +
											"\t\t\t" + "{0} = null;" + "\r\n" +
											"\t\t" + "} else {" + "\r\n" +
											"\t\t\t" + "int count_{0} = src.{0}.Count;" + "\r\n" +
											"\t\t\t" + "{0}.Clear();" + "\r\n" +
											"\t\t\t" + "for( int i=0; i<count_{0}; i++ ) {" + "\r\n" +
											"\t\t\t\t" + "{2} data_{0} = new {2}();" + "\r\n" +
											"\t\t\t\t" + "data_{0}.CopyFrom( src.{0}[i] );" + "\r\n" +
											"\t\t\t\t" + "{0}.Add( data_{0} );" + "\r\n" +
											"\t\t\t" + "}" + "\r\n" +
											"\t\t" + "}"
										,
											"\t\t" + "if( src.{0} == null ) {" + "\r\n" +
											"\t\t\t" + "{0} = null;" + "\r\n" +
											"\t\t\t" + "} else {" + "\r\n" +
											"\t\t\t" + "int count_{0} = src.{0}.Count;" + "\r\n" +
											"\t\t\t" + "{0}.Clear();" + "\r\n" +
											"\t\t\t" + "for( int i=0; i<count_{0}; i++ ) {" + "\r\n" +
											"\t\t\t\t" + "{0}.Add( src.{0}[i] );" + "\r\n" +
											"\t\t\t" + "}" + "\r\n" +
											"\t\t" + "}"
										};

	static string [] _copy_variable_type = {
											"\t\t{0} = src.{0};"
										,
											"\t\t" + "if( src.{0} != null ) {" + "\r\n" +
											"\t\t\t" + "if( {0} == null ) {0} = new {2}();" + "\r\n" +
											"\t\t\t" + "{0}.CopyFrom( src.{0} );" + "\r\n" + 
											"\t\t" + "} else {" + "\r\n" + 
											"\t\t\t" + "{0} = null;" + "\r\n" +
											"\t\t" + "}"
										,
											"\t\t{0} = src.{0};"
										};
	////////////////////////////////////////////////////////////////////////////////////////////
}
