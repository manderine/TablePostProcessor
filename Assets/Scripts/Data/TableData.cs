using UnityEngine;
using System.Collections;
using System.Collections.Generic;
// io
using System.IO;

public class jUtil {
	public readonly static string _ResPathName = "Assets/Resources/";

	public static GameObject NewUI( string path, Transform parent ) {
		GameObject obj = new GameObject();
		if( obj != null ) {
			Transform form = obj.transform;
			form.name = path;

			form.SetParent( parent );

			form.localPosition = Vector3.zero;
			form.localRotation = Quaternion.identity;
			form.localScale = Vector3.one;
		}
		return obj;
	}

	static string _DirectoryReplace( string path ) {
		path = path.Replace( "\\", "/" );
		int find = path.LastIndexOf( _ResPathName );
        if( find > 0 ) {
		    path = path.Substring( find + _ResPathName.Length );
        }
		return path;
	}

    static void _DirectoryFiles( List<string> lst, DirectoryInfo dir, string ext ) {
		FileInfo [] fis = dir.GetFiles( ext );
		if( fis != null ) {
			for( int i=0; i<fis.Length; i++ ) {
				FileInfo fi = fis[i];
				if( fi.Extension.CompareTo( ".meta" ) == 0 ) {
					continue;
				}
				string str = _DirectoryReplace( fi.DirectoryName ) + "/" + GetFileTitle( fi.Name );
				lst.Add( str );
			}
			fis = null;
		}

		DirectoryInfo [] dis = dir.GetDirectories();
		if( dis != null ) {
			for( int i=0; i<dis.Length; i++ ) {
				DirectoryInfo di = dis[i];
				_DirectoryFiles( lst, di, ext );
			}
			dis = null;
		}
    }

    public static void GetFiles( List<string> lst, string path, string ext ) {
        DirectoryInfo dir = new DirectoryInfo( path );
        _DirectoryFiles( lst, dir, ext );
        dir = null;
    }
	
	public static string GetFileTitle( string path ) {
		// Assets/Resources/Enemy/pfGoblinGreen.prefab -> pfGoblinGreen
		return Path.GetFileNameWithoutExtension( path );
	}
}

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
	protected static T __instance = null;
	public static T _instance {
		get {
			if( __instance == null ) {
				__instance = FindObjectOfType( typeof(T) ) as T;
				if( __instance == null ) {
					GameObject go = jUtil.NewUI( typeof(T).Name, null );
					//DontDestroyOnLoad( go );
					__instance = go.AddComponent<T>();
				}
			}
			return __instance;
		}
	}

	public static void Destroy() {
		if( __instance != null ) {
			GameObject.DestroyObject( __instance.gameObject );
			__instance = null;
		}
	}
}

public class SingletonEx<T> : MonoBehaviour where T : MonoBehaviour {
	protected static T __instance = null;
	public static T _instance {
		get {
			if( __instance == null ) {
				__instance = FindObjectOfType( typeof(T) ) as T;
				if( __instance == null ) {
					GameObject go = jUtil.NewUI( typeof(T).Name, null );
					DontDestroyOnLoad( go );
					__instance = go.AddComponent<T>();
				}
			}
			return __instance;
		}
	}

	public static void Destroy() {
		if( __instance != null ) {
			GameObject.DestroyObject( __instance.gameObject );
			__instance = null;
		}
	}
}

public partial class TableData {
	public int _ID;

	public TableData() {
		_ID = 0;
	}

	public virtual void UnpackData( jSerializerReader sr ) {
		_ID = sr.ReadInt32();
	}

	public virtual void PackData( jSerializerWriter sw ) {
		sw.Write( _ID );
	}

	public virtual void CopyFrom( object source ) {
		TableData src = source as TableData;
		_ID = src._ID;
	}
}

//<Binary_Pack>
public partial class TableList<T> where T : TableData, new() {
	//<Binary_Pack_Start>
	public List<T> _DataList;
	//<Binary_Pack_End>

	public void Add( T data ) {
		for( int i=0; i<_DataList.Count; i++ )  {
			if( data._ID < _DataList[i]._ID ) {
				_DataList.Insert( i, data );
				return;
			}
		}

		_DataList.Add( data );
	}

	public int IndexOf( int id ) {
		int low = 0, high = _DataList.Count - 1, mid;

		while( low <= high ) {
			mid = (low + high) / 2;
			if( _DataList[mid]._ID > id ) {
				high = mid - 1;
			} else if( _DataList[mid]._ID < id ) {
				low = mid + 1;
			} else {
				return mid;
			}
		}

		return -1;
	}

	public T Find( int id ) {
		int find;
		if( (find = IndexOf( id )) >= 0 ) {
			return _DataList[find];
		}
		return null;
	}

	public void Sort() {
		T tmp;
		for( int i=0; i<(_DataList.Count - 1); i++ ) {
			for( int j=(i + 1); j<_DataList.Count; j++ ) {
				if( _DataList[i]._ID > _DataList[j]._ID ) {
					tmp = _DataList[i] ;
					_DataList[i] = _DataList[j];
					_DataList[j] = tmp as T;
				}
			}
		}
	}
}

//<Binary_Pack>
public partial class StringData : TableData {
	//<Binary_Pack_Start>
	public string _message;
	//<Binary_Pack_End>

	public StringData() {
		_message = "";
	}
}

public class StringTable : TableList<StringData> {
	public StringTable() {
		_DataList = new List<StringData>();
	}
}

//<Binary_Pack>
public partial class AnimationCurveData : TableData {
	//<Binary_Pack_Start>
	public string _desc;
	public AnimationCurve _curve;
	//<Binary_Pack_End>

	public AnimationCurveData() {
		_ID = 0;

		_desc = "";

		_curve = new AnimationCurve( new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f) );
	}
}

public class AnimationCurveTable : TableList<AnimationCurveData> {
	public AnimationCurveTable() {
		_DataList = new List<AnimationCurveData>();
	}
}
