using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class Repro : MonoBehaviour
{
	private void Awake()
	{
		string path              = "watched";
		string subdir            = "subdir";
		string file_name         = "WillChange.cs";
		string sub_file_name     = "WillChangeSub.cs";
		string watcher_extension = "cs";

		watcher_path  = Path.GetFullPath( Path.Combine( Application.dataPath, path ) );
		file_path     = Path.GetFullPath( Path.Combine( watcher_path, file_name ) );
		sub_file_path = Path.GetFullPath( Path.Combine( watcher_path, subdir, sub_file_name ) );

		Debug.Log( $"watcher  path: '{watcher_path}'" );
		Debug.Log( $"file     path: '{file_path}'" );
		Debug.Log( $"sub file path: '{sub_file_path}'" );

		fw = new FileSystemWatcher()
		{
			Path = watcher_path,
			NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
			IncludeSubdirectories = true,
			Filter = $"*.{watcher_extension}",
			EnableRaisingEvents = true,
		};
		fw.Changed += handle_changed;

		StartCoroutine( coroutine() );
	}
	private IEnumerator coroutine()
	{
		while ( true )
		{
			yield return new WaitForSecondsRealtime( 1 );
			change( file_path );
			change( sub_file_path );
		}
	}
	private FileSystemWatcher fw;
	private string watcher_path;
	private string sub_file_path;
	private string file_path;

	private void change( string file_path )
	{
		string file_content = File.ReadAllText( file_path, Encoding.UTF8 );
		string trimmed = file_content.TrimEnd( '\n' );
		Debug.Log( $"trimmed:\n'{trimmed}'" );
		int last_index_of = trimmed.LastIndexOf( "\n" );
		Debug.Log( $"last_index_of: {last_index_of}" );
		file_content = file_content.Substring( 0, last_index_of );
		Debug.Log( $"file content minus last line:\n'{file_content}'" );
		file_content += $"\n// added line {DateTime.Now:HHHH-mm-dd_hh-MM-ss}";
		Debug.Log( $"file content w/ added line:\n'{file_content}'" );
		File.WriteAllText( file_path, file_content );
	}

	public void handle_changed( object sender, FileSystemEventArgs e )
	{
		Debug.Log( $"changeType: {e.ChangeType} directory: {e.FullPath} name: {e.Name}" );
		if ( e.FullPath != file_path && e.FullPath != sub_file_path )
		{
			Debug.LogError( $"Unrecognized e.FullPath:\n"
			              + $"e.FullPath             : '{e.FullPath}'\n"
			              + $"watcher  path          : '{watcher_path}'\n"
			              + $"file     path          : '{file_path}'\n"
			              + $"sub file path          : '{sub_file_path}'" );
			if ( e.FullPath == Path.Combine( watcher_path, e.Name ) )
			{
				Debug.LogError( $"FullPath is watcher_path + e.Name: '{e.FullPath}'" );
			}
		}
	}
}
