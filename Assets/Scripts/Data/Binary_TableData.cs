using UnityEngine;
using System;

public partial class TableList<T> where T : TableData, new() {
	public virtual void UnpackData( jSerializerReader sr ) {
		int count__DataList = sr.ReadInt32();
		_DataList.Clear();
		for( int i=0; i<count__DataList; i++ ) {
			T data__DataList = new T();
			data__DataList.UnpackData( sr );
			_DataList.Add( data__DataList );
		}
	}

	public virtual void PackData( jSerializerWriter sw ) {
		if( _DataList == null ) {
			sw.Write( 0 );
		} else {
			sw.Write( (int)_DataList.Count );
			for( int i=0; i<_DataList.Count; i++ ) {
				_DataList[i].PackData( sw );
			}
		}
	}

	public virtual void CopyFrom( object source ) {
		TableList<T> src = source as TableList<T>;
		if( src._DataList == null ) {
			_DataList = null;
		} else {
			int count__DataList = src._DataList.Count;
			_DataList.Clear();
			for( int i=0; i<count__DataList; i++ ) {
				T data__DataList = new T();
				data__DataList.CopyFrom( src._DataList[i] );
				_DataList.Add( data__DataList );
			}
		}
	}
}

public partial class StringData : TableData {
	public override void UnpackData( jSerializerReader sr ) {
		base.UnpackData( sr );
		_message = sr.ReadString();
	}

	public override void PackData( jSerializerWriter sw ) {
		base.PackData( sw );
		sw.Write( _message );
	}

	public override void CopyFrom( object source ) {
		StringData src = source as StringData;
		base.CopyFrom( src );
		_message = src._message;
	}
}

public partial class AnimationCurveData : TableData {
	public override void UnpackData( jSerializerReader sr ) {
		base.UnpackData( sr );
		_desc = sr.ReadString();
		_curve = sr.ReadAnimationCurve();
	}

	public override void PackData( jSerializerWriter sw ) {
		base.PackData( sw );
		sw.Write( _desc );
		sw.Write( _curve );
	}

	public override void CopyFrom( object source ) {
		AnimationCurveData src = source as AnimationCurveData;
		base.CopyFrom( src );
		_desc = src._desc;
		_curve = src._curve;
	}
}

