from netCDF4 import Dataset
import sys 

#ocean_time = int(sys.argv[1])  
#s_rho = int(sys.argv[2])
#eta_rho = int(sys.argv[3])
#xi_rho = int(sys.argv[4])
#heatModellPath = str(sys.argv[5])
#ds = Dataset(heatModellPath, "r", format="NETCDF4")

#print(ds.variables["temp"][ocean_time, s_rho, eta_rho, xi_rho])

ocean_time = 273
s_rho = 5
eta_rho = 901
xi_rho = 2601
heatModellPath = "C:/NCdata/VarmeModell/norkyst_800m_avg.nc"
ds = Dataset(heatModellPath, "r", format="NETCDF4")

print("HAllo")
print(ds.variables["temp"][ocean_time, s_rho, eta_rho, xi_rho])
