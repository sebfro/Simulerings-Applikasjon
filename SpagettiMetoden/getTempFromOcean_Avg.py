from netCDF4 import Dataset
import sys 

ocean_time = int(sys.argv[1])  
s_rho = int(sys.argv[2])
eta_rho = int(sys.argv[3])
xi_rho = int(sys.argv[4])
heatModellPath = str(sys.argv[5])
ds = Dataset(heatModellPath, "r", format="NETCDF4")

print(ds.variables["temp"][ocean_time, s_rho, eta_rho, xi_rho])
