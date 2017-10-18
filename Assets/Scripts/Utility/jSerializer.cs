using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class jSerializerReader : BinaryReader {
	public jSerializerReader( Stream s ) : base ( s ) { }

	public Vector3 ReadVector3() {
		Vector3 v = Vector3.zero;
		v.x = base.ReadSingle();
		v.y = base.ReadSingle();
		v.z = base.ReadSingle();
		return v;
	}

	public Vector4 ReadVector4() {
		Vector4 v = Vector4.zero;
		v.x = base.ReadSingle();
		v.y = base.ReadSingle();
		v.z = base.ReadSingle();
		v.w = base.ReadSingle();
		return v;
	}

	public Color ReadColor() {
		Color c = Color.black;
		c.r = base.ReadSingle();
		c.g = base.ReadSingle();
		c.b = base.ReadSingle();
		c.a = base.ReadSingle();
		return c;
	}

	public AnimationCurve ReadAnimationCurve() {
		int count = base.ReadInt32();

		Keyframe [] keys = new Keyframe[ count ];
		for( int i=0; i<count; i++ ) {
			float inTangent = base.ReadSingle();
			float outTangent = base.ReadSingle();
			int tangentMode = base.ReadInt32();
			float time = base.ReadSingle();
			float value = base.ReadSingle();

			keys[i] = new Keyframe( time, value, inTangent, outTangent );
			keys[i].tangentMode = tangentMode;
		}

		AnimationCurve ac = new AnimationCurve( keys );
		ac.postWrapMode = (WrapMode)base.ReadInt32();
		ac.preWrapMode = (WrapMode)base.ReadInt32();

		return ac;
	}

	public byte [] ReadBytes() {
		int len = ReadInt32();
		if( len > 0 ) {
			return ReadBytes( len );
		}
		return new byte[0];
	}

	public System.DateTime ReadDateTime() {
		return new System.DateTime( ReadInt64() );
	}
}

public class jSerializerWriter : BinaryWriter {
	public jSerializerWriter( Stream s ) : base ( s ) { }

	public override void Write( string value ) {
		if( value == null ) {
			value = "";
		}
		base.Write( value );
	}

	public void Write( Vector3 v3 ) {
		base.Write( v3.x );
		base.Write( v3.y );
		base.Write( v3.z );
	}

	public void Write( Vector4 v4 ) {
		base.Write( v4.x );
		base.Write( v4.y );
		base.Write( v4.z );
		base.Write( v4.w );
	}

	public void Write( Color c ) {
		base.Write( c.r );
		base.Write( c.g );
		base.Write( c.b );
		base.Write( c.a );
	}

	public void Write( AnimationCurve ac ) {
		Keyframe [] keys = ac.keys;

		base.Write( keys.Length );
		for( int i=0; i<keys.Length; i++ ) {
			base.Write( keys[i].inTangent );
			base.Write( keys[i].outTangent );
			base.Write( keys[i].tangentMode );
			base.Write( keys[i].time );
			base.Write( keys[i].value );
		}

		base.Write( (int)ac.postWrapMode );
		base.Write( (int)ac.preWrapMode );
	}

	public override void Write( byte [] buffer ) {
		int len = buffer.Length;
		base.Write( len );
		if( len > 0 ) {
			base.Write( buffer );
		}
	}

	public void Write( System.DateTime date ) {
		base.Write( date.Ticks );
	}
}
