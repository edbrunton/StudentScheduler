namespace StudentScheduler
{
	/// <summary>
	/// A boolean value that allows for multi-threaded lock handling
	/// </summary>
	public class InterlockingBool
	{
		/// <summary>
		/// The value of the bool
		/// </summary>
		private bool Val { get; set; } = false;
		/// <summary>
		/// Is the value intialized? 
		/// </summary>
		private bool Initialized { get; set; } = false;
		public InterlockingBool()
		{
			Initialized = false;
		}
		public InterlockingBool(bool startingVal)
		{
			SetValue(startingVal);
			Initialized = true;
		}
		public bool GetValue()
		{
			lock (this)
			{
				return Val;
			}
		}
		public bool SetValue(bool v)
		{
			lock (this)
			{
				Val = v;
				Initialized = true;
				return v;
			}
		}
		public bool IsInitialized()
		{
			lock (this)
			{
				return Initialized;
			}
		}
	}
}
