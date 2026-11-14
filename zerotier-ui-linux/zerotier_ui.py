#!/usr/bin/env python3
import json, os, subprocess
from pathlib import Path
import gi
gi.require_version("Gtk", "4.0")
from gi.repository import Gtk, GLib

CONFIG = Path(os.environ.get("XDG_CONFIG_HOME", Path.home() / ".config")) / "zerotier-ui" / "networks.json"
CLI = "zerotier-cli"

class App(Gtk.Application):
    def __init__(self):
        super().__init__(application_id="io.github.kernelsyLabs.ZeroTierUI")
        self.networks = {}

    def do_activate(self):
        self.win = Gtk.ApplicationWindow(application=self, title="ZeroTier UI", default_width=720, default_height=500)
        root = Gtk.Box(orientation=Gtk.Orientation.VERTICAL, spacing=10)
        root.set_margin_top(14); root.set_margin_bottom(14); root.set_margin_start(14); root.set_margin_end(14)
        heading = Gtk.Box(orientation=Gtk.Orientation.HORIZONTAL, spacing=10)
        title = Gtk.Label(label="ZeroTier networks", xalign=0); title.add_css_class("title-2"); heading.append(title)
        refresh = Gtk.Button(label="Refresh"); refresh.connect("clicked", lambda _: self.load()); heading.append(refresh)
        add = Gtk.Button(label="Add ID"); add.connect("clicked", lambda _: self.add_dialog()); heading.append(add)
        root.append(heading)
        self.listbox = Gtk.ListBox(); self.listbox.set_show_separators(True)
        scroll = Gtk.ScrolledWindow(vexpand=True); scroll.set_child(self.listbox); root.append(scroll)
        self.status = Gtk.Label(xalign=0); root.append(self.status)
        self.win.set_child(root); self.win.present(); self.load()

    def load_saved(self):
        try: return json.loads(CONFIG.read_text())
        except Exception: return {}

    def save(self):
        CONFIG.parent.mkdir(parents=True, exist_ok=True); CONFIG.write_text(json.dumps(self.networks, indent=2) + "\n")

    def cli(self, *args):
        return subprocess.run([CLI, *args], text=True, capture_output=True, check=True).stdout

    def load(self):
        self.networks = self.load_saved()
        try:
            for n in json.loads(self.cli("-j", "listnetworks")):
                self.networks[n["nwid"]] = {"name": n.get("name") or n["nwid"], "status": n.get("status", "")}
        except Exception as e: self.status.set_text(f"ZeroTier error: {e}")
        self.save(); self.render()

    def render(self):
        while child := self.listbox.get_first_child(): self.listbox.remove(child)
        for nid, item in sorted(self.networks.items()):
            active = item.get("status", "") not in ("", "DISABLED")
            row = Gtk.Box(orientation=Gtk.Orientation.HORIZONTAL, spacing=12); row.set_margin_top(10); row.set_margin_bottom(10); row.set_margin_start(10); row.set_margin_end(10)
            labels = Gtk.Box(orientation=Gtk.Orientation.VERTICAL, hexpand=True)
            labels.append(Gtk.Label(label=item.get("name", nid), xalign=0)); labels.append(Gtk.Label(label=f"{nid}  •  {item.get('status', 'DISABLED')}", xalign=0))
            row.append(labels); button = Gtk.Button(label="Disable" if active else "Enable"); button.connect("clicked", self.toggle, nid, active); row.append(button)
            self.listbox.append(row)
        self.status.set_text(f"{len(self.networks)} network(s) • config: {CONFIG}")

    def toggle(self, _, nid, active):
        try: self.cli("leave" if active else "join", nid); self.load()
        except Exception as e: self.error(str(e))

    def add_dialog(self):
        dialog = Gtk.Dialog(title="Add ZeroTier network", transient_for=self.win, modal=True); dialog.add_button("Cancel", Gtk.ResponseType.CANCEL); dialog.add_button("Add", Gtk.ResponseType.OK)
        box = dialog.get_content_area(); box.set_spacing(8); box.set_margin_top(12); box.set_margin_bottom(12); box.set_margin_start(12); box.set_margin_end(12)
        entry = Gtk.Entry(); entry.set_placeholder_text("16-character network ID"); box.append(entry); dialog.connect("response", self.add_response, entry); dialog.present()

    def add_response(self, dialog, response, entry):
        if response == Gtk.ResponseType.OK:
            nid = entry.get_text().strip().lower()
            if len(nid) == 16 and all(c in "0123456789abcdef" for c in nid): self.networks[nid] = {"name": nid, "status": "DISABLED"}; self.save(); self.render()
            else: self.error("Network ID harus 16 karakter hexadecimal")
        dialog.destroy()

    def error(self, message):
        d = Gtk.AlertDialog(message=message); d.show(self.win)

App().run()
