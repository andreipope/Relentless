import xmlrunner
from appium import webdriver

from altunityrunner import AltrunUnityDriver, NotFoundException
from .pages.area_bar_popup_page import Area_Bar_Popup_Page
from .pages.base import CZBTests
from .pages.forgot_password_page import Forgot_Password_Page
from .pages.login_popup_page import Login_Popup_Page
from .pages.main_menu_page import Main_Menu_Page
from .pages.register_popup_page import Regitration_Popup_Page
from .pages.succes_forgot_page import Succes_Forgot_Page
from .pages.wait_page import Wait_Page
from .pages.side_menu_popup_page import Side_Menu_Popup_Page
from .pages.forgot_password_page import Forgot_Password_Page
from .pages.horde_editing_page import Horde_Editing_Page
from .pages.overlord_selection_page import Overlord_Selection_Page
from .pages.overlord_ability_popup_page import Overlord_Ability_Popup_Page
from .pages.deck_rename_tab_page import Deck_Rename_Tab_Page
from .pages.battle_mode_selection_page import Battle_Mode_Selection_Page
from .pages.deck_selection_page import Deck_Selection_Page
from .pages.tutorial_popup_page import Tutorial_Popup_Page
from .pages.open_packs_page import Open_Packs_Page
from .pages.battle_mode_selection_pop_up_page import Battle_Mode_Selection_Popup_Page
from .pages.settings_page import Settings_Page
from .pages.credits_page import Credits_Page
from .pages.my_cards_page import My_Cards_Page
from .pages.shop_page import Shop_Page
driver=None
altdriver=None
class TestModel(CZBTests):

	def setUpModel(self):
		global driver
		global altdriver
		super().setUp()
		driver=self.driver
		altdriver=self.altdriver

	def tutorial_popup_page(self):
		self.tutorial_popup_page=Tutorial_Popup_Page(altdriver)

	def side_menu_page(self):
		self.side_menu_popup_page=Side_Menu_Popup_Page(altdriver)

	def main_menu_page(self):
		self.menu_page=Main_Menu_Page(altdriver)

	def area_bar_page(self):
		self.area_bar_popup_page=Area_Bar_Popup_Page(altdriver)

	def my_shop_menu_page(self):
		pass
		# self.shop_page=Shop_Page(altdriver,driver)

	def my_decks_page(self):
		self.deck_selection_page=Deck_Selection_Page(altdriver)

	def my_packs_page(self):
		self.open_packs_page=Open_Packs_Page(altdriver,driver)

	def my_cards_page(self):
		self.my_card_page=My_Cards_Page(altdriver,driver)

	def press_no_button(self):
		self.tutorial_popup_page.press_No_button()

	def select_area_bar(self):
		pass

	def select_side_menu(self):
		pass

	def press_my_decks_button(self):
		self.side_menu_popup_page.press_my_decks_button()

	def press_main_menu_page(self):
		self.side_menu_popup_page.press_battle_button()

	def press_my_cards_button(self):
		self.side_menu_popup_page.press_my_cards_button()

	def press_my_shop_button(self):
		pass
		# self.side_menu_popup_page.press_shop_button()

	def press_open_packs_button(self):
		self.side_menu_popup_page.press_open_packs_button()

	def return_from_login_model(self):
		pass


class LoginModel:

	def area_bar_page(self):
		pass

	def settings_page(self):
		pass

	def credits_page(self):
		pass

	def login_popup_page(self):
		pass

	def register_popup_page(self):
		pass

	def forgot_password_page(self):
		pass

	def press_register_button(self):
		pass

	def press_forgot_password_button(self):
		pass

	def fill_and_send_forgot_password_form(self):
		pass

	def fill_and_send_register_form(self):
		pass

	def login_with_valid_account(self):
		pass

	def press_setting_button(self):
		pass

	def press_credits_button(self):
		pass

	def press_close_settings(self):
		pass

	def press_close_settings(self):
		pass


class MyDeckModel:

	def exit_my_deck(self):
		pass

	def my_decks_page(self):
		pass

	def overlord_selection_page(self):
		pass

	def overlord_ability_popup_page(self):
		pass

	def rename_deck_popup_page(self):
		pass

	def horde_editing_page(self):
		pass

	def create_new_deck(self):
		pass

	def press_overlord_selection_page_continue_button(self):
		pass

	def press_ooverlord_ability_popup_page_continue_button(self):
		pass

	def press_save_button(self):
		pass

	def add_card_to_deck(self):
		pass

	def press_save_deck_button(self):
		pass

	def press_rename_button(self):
		pass

	def select_overlord(self):
		pass

	def press_back_button_from_overlord_selection_page(self):
		pass

	def press_back_button_from_overlord_ability_page(self):
		pass

	def press_back_button_from_horde_editing_page(self):
		pass

	def press_rename_from_horde_editing_page(self):
		pass

	def go_to_navigation_page(self):
		pass

	def go_to_navigation_page(self):
		pass

	def go_to_navigation_page(self):
		pass

	def go_to_navigation_page(self):
		pass

	def go_to_navigation_page(self):
		pass


class BattleModel:

	def main_menu_page(self):
		pass

	def battle_mode_selection_pop_up_page(self):
		pass

	def press_select_game_mode(self):
		pass

	def select_solo_mode(self):
		pass

	def select_pvp_mode(self):
		pass

	def start_tutorial(self):
		pass

	def start_game(self):
		pass

