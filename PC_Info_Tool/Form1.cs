using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Management.Instrumentation;
using System.Net;


namespace PC_Info_Tool {
	public partial class Form1:Form {

		#region Declarations and Vars
		string sPCName, sManufacturer, sRam, sModelName, sSN;
		bool RealClose = false;
		int iNetCards = 0;
		//TODO get netowrk adapter formatting
		#endregion

		public Form1() {
			if( System.Diagnostics.Process.GetProcessesByName( System.IO.Path.GetFileNameWithoutExtension( System.Reflection.Assembly.GetEntryAssembly().Location ) ).Count()>1 ) {
				System.Diagnostics.Process.GetCurrentProcess().Kill();
			}
			InitializeComponent();
			RefreshContent();
		}

		private void RefreshContent() {
			int iNetCards = 0;
			richTextBox1.Text="";
			try {
				ManagementObjectSearcher serobjWin32CS = new ManagementObjectSearcher( "root\\CIMV2","SELECT * FROM Win32_ComputerSystem" );
				foreach( ManagementObject queryObj1 in serobjWin32CS.Get() ) {
					sPCName=queryObj1["DNSHostName"].ToString();
					sManufacturer=queryObj1["Manufacturer"].ToString();
					//TODO: need to convert to int and divide for gb
					sRam= queryObj1["TotalPhysicalMemory"].ToString();
				}
				ManagementObjectSearcher serobjWin32CSP = new ManagementObjectSearcher( "root\\CIMV2","SELECT * FROM Win32_ComputerSystemProduct" );
				foreach( ManagementObject queryObj2 in serobjWin32CSP.Get() ) {
					sModelName=queryObj2["Name"].ToString();
					sSN=queryObj2["IdentifyingNumber"].ToString();
				}
				richTextBox1.Text="Computer Name: ";
				richTextBox1.Select( richTextBox1.Text.Length,sPCName.Length );
				richTextBox1.SelectionFont=new Font( richTextBox1.SelectionFont.FontFamily,14.0f,FontStyle.Bold );
				richTextBox1.AppendText( sPCName+"\n" );
				richTextBox1.AppendText( "Username: "+Environment.UserName+"\n" );
				richTextBox1.Select( richTextBox1.Text.Length-( Environment.UserName.Length+1 ),Environment.UserName.Length );
				richTextBox1.SelectionFont=new Font( richTextBox1.SelectionFont.FontFamily,14.0f,FontStyle.Regular );
				richTextBox1.AppendText( "Manufacturer: " );
				richTextBox1.AppendText( sManufacturer+" - " );
				richTextBox1.AppendText( sModelName+"\n" );
				richTextBox1.AppendText( "Serial Number: " );
				richTextBox1.AppendText( sSN+"\n" );
				richTextBox1.AppendText( "Total Physical Memory: " );
				richTextBox1.AppendText( sRam+"\n" );
				richTextBox1.AppendText("==========================================\n\n" );

				//Network Cards Iterations
				ManagementObjectSearcher objSearchWin32NetAdpCfg = new ManagementObjectSearcher( "root\\CIMV2","SELECT * FROM Win32_NetworkAdapterConfiguration" );
				foreach( ManagementObject queryObj3 in objSearchWin32NetAdpCfg.Get() ) {
					if( queryObj3["IPAddress"]!=null ) {
						richTextBox1.AppendText( "NIC name: "+queryObj3["Description"].ToString()+'\n' );
						richTextBox1.AppendText( "IPv4 Address: " );
						String[] arrIPAddress = ( String[] )( queryObj3["IPAddress"] );
						richTextBox1.Select( richTextBox1.Text.Length,arrIPAddress.Length );
						richTextBox1.SelectionFont=new Font( richTextBox1.SelectionFont.FontFamily,14.0f,FontStyle.Bold );
						richTextBox1.AppendText( arrIPAddress[0].ToString()+"\n" );
						//some VPN's have no listed DHCP server like cisco secure mobility client
						if( queryObj3["DHCPServer"]!=null ) {
							richTextBox1.AppendText( "DHCPServer: "+queryObj3["DHCPServer"].ToString()+'\n' );
						}
						richTextBox1.AppendText( "MAC Address: "+queryObj3["MACAddress"].ToString()+'\n' );
						if( queryObj3["DNSServerSearchOrder"]!=null ) {
							richTextBox1.AppendText( "DNS Servers: " );
							String[] arrDNSServerSearchOrder = ( String[] )( queryObj3["DNSServerSearchOrder"] );
							foreach( String arrValue in arrDNSServerSearchOrder ) {
								richTextBox1.AppendText( arrValue+"  " );
							}
						}
						richTextBox1.AppendText( "\n" );
						iNetCards++;
					}
				}
				objSearchWin32NetAdpCfg.Dispose();
			} catch( ManagementException err ) {
				MessageBox.Show( err.Message,"error pulling data",MessageBoxButtons.OK,MessageBoxIcon.Error );
			}
			if( iNetCards==0 ) {
				richTextBox1.AppendText( "No active network connections.\nConnect to Wifi or Ethernet and refresh from the File menu." );
			}else{
				richTextBox1.AppendText( "\nActive NIC Cards: "+iNetCards.ToString() );
			}
		}

		#region MenuStrip

		#endregion
		#region systray functions
		private void notifyIcon1_DoubleClick( object sender,EventArgs e ) {
			Show();
			WindowState=FormWindowState.Normal;
			RefreshContent();
		}

		private void displayToolStripMenuItem_Click( object sender,EventArgs e ) {
			Show();
			WindowState=FormWindowState.Normal;
			RefreshContent();
		}

		private void exitToolStripMenuItem_Click( object sender,EventArgs e ) {
			RealClose=true;
			Close();
		}
		
		private void Form1_FormClosing( Object sender,FormClosingEventArgs e ) {
			if( RealClose==true ) {
				this.Dispose();
			} else {
				e.Cancel=true;
				Hide();
				WindowState=FormWindowState.Minimized;
				notifyIcon1.Visible=true;
			}
		}

		private void Form1_Resize( Object sender,EventArgs e ) {
			if( WindowState==FormWindowState.Minimized ) {
				Hide();
				notifyIcon1.Visible=true;
			}
		}
		#endregion
	}
}